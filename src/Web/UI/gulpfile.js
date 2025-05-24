// Gulpfile with Vue 3 SFC compilation support (fixed double export issue)
const { series, src, dest, parallel, watch } = require("gulp");
const autoprefixer = require("gulp-autoprefixer");
const browsersync = require("browser-sync");
const babel = require('gulp-babel');
const concat = require("gulp-concat");
const CleanCSS = require("gulp-clean-css");
const del = require("del");
const fileinclude = require("gulp-file-include");
const newer = require("gulp-newer");
const rename = require("gulp-rename");
const rtlcss = require("gulp-rtlcss");
const sourcemaps = require("gulp-sourcemaps");
const sass = require("gulp-sass")(require("sass"));
const uglify = require("gulp-uglify");
const { parse, compileScript, compileTemplate } = require("@vue/compiler-sfc");
const through2 = require("through2");
const path = require("path");
const Vinyl = require("vinyl");

const pluginFile = require("./plugins.config");

const paths = {
    baseSrc: "src/",
    baseDist: "dist/",
    vueSrc: "src/vue/",
    components: "src/vue/components/"
};

const clean = function (done) {
    del.sync(paths.baseDist, done());
};

const plugins = function () {
    const out = paths.baseDist + "plugins/";
    pluginFile.forEach(({ name, assets, fonts, font, media, img, templates, webfonts }) => {
        if (assets) {
            assets.forEach((file) => {
                let folderType = file.endsWith(".css") ? "css" : "js";
                src(file).pipe(dest(`${out}${name}/${folderType}/`));
            });
        }
        if (fonts) src(fonts).pipe(dest(`${out}${name}/css/fonts/`));
        if (font) src(font).pipe(dest(`${out}${name}/css/font/`));
        if (img) src(img).pipe(dest(`${out}${name}/img/`));
        if (media) src(media).pipe(dest(`${out}${name}/css/`));
        if (webfonts) src(webfonts).pipe(dest(`${out}${name}/webfonts/`));
    });
    return Promise.resolve();
};

const html = function () {
    return src([paths.baseSrc + "*.html", paths.baseSrc + "*.ico", paths.baseSrc + "*.png"])
        .pipe(fileinclude({ prefix: "@@", basepath: "@file", indent: true }))
        .pipe(dest(paths.baseDist));
};

const locales = function () {
    src([paths.baseSrc + "pdf/**/*"]).pipe(dest(paths.baseDist + "pdf/"));
    return src([paths.baseSrc + "locales/**/*"]).pipe(dest(paths.baseDist + "locales/"));
};

const fonts = function () {
    return src([paths.baseSrc + "fonts/**/*"]).pipe(newer(paths.baseDist + "fonts/"))
        .pipe(dest(paths.baseDist + "fonts/"));
};

const images = function () {
    return src(paths.baseSrc + "img/**/*").pipe(dest(paths.baseDist + "img"));
};

const templates = function () {
    return src(paths.baseSrc + "templates/**/*", { allowEmpty: true }).pipe(dest(paths.baseDist + "templates"));
};

const javascript = function () {
    return src(paths.baseSrc + "js/**/*.js").pipe(uglify()).pipe(dest(paths.baseDist + "js/"));
};

const vueScripts = function() {
    return src(paths.vueSrc + "**/*.js")
        .pipe(uglify())
        .pipe(dest(paths.baseDist + "js/"));
};

const vue = function () {
    const outDir = paths.baseDist + "components/";
    return src(paths.components + "**/*.vue")
        .pipe(through2.obj(function (file, _, cb) {
            const { descriptor, errors } = parse(file.contents.toString(), { filename: file.path });

            if (errors.length > 0) {
                return cb(new Error(`Vue SFC parse error in ${file.path}:
${errors.join("\n")}`));
            }

            const hasScript = !!descriptor.script || !!descriptor.scriptSetup;
            let scriptContent = hasScript
                ? compileScript(descriptor, { id: path.basename(file.path) }).content
                : "const script = {};";

            if (scriptContent.includes("export default")) {
                scriptContent = scriptContent.replace(/export default/, "const script =");
            }

            const templateResult = compileTemplate({
                source: descriptor.template?.content || "<div></div>",
                filename: file.path,
                id: 'data-v-' + Math.random().toString(36).substr(2, 8),
            });

            if (templateResult.errors.length) {
                return cb(new Error(`Template compile error: ${templateResult.errors.join("\n")}`));
            }

            const finalCode = `
${scriptContent}
${templateResult.code}
script.render = render;
export default script;
`;

            const compiledFile = new Vinyl({
                cwd: file.cwd,
                base: file.base,
                path: file.path.replace(/\.vue$/, ".js"),
                contents: Buffer.from(finalCode)
            });

            cb(null, compiledFile);
        }))
        .pipe(dest(outDir));
};

const scss = function () {
    const out = paths.baseDist + "css/";
    return src(paths.baseSrc + "scss/**/*.scss")
        .pipe(sourcemaps.init())
        .pipe(sass.sync().on('error', sass.logError))
        .pipe(autoprefixer({ overrideBrowserslist: ["last 2 versions"] }))
        .pipe(dest(out))
        .pipe(CleanCSS())
        .pipe(rename({ suffix: ".min" }))
        .pipe(sourcemaps.write("./"))
        .pipe(dest(out));
};

const initBrowserSync = function (done) {
    browsersync.init({
        startPath: "/index.html",
        server: {
            baseDir: paths.baseDist,
            middleware: [function (req, res, next) { req.method = "GET"; next(); }]
        },
    });
    done();
};

const reloadBrowserSync = function (done) {
    browsersync.reload();
    done();
};

function watchFiles() {
    watch(paths.baseSrc + "**/*.html", series(html, reloadBrowserSync));
    watch(paths.baseSrc + "locales/**/*", series(locales, reloadBrowserSync));
    watch(paths.baseSrc + "fonts/**/*", series(fonts, reloadBrowserSync));
    watch(paths.baseSrc + "img/**/*", series(images, reloadBrowserSync));
    watch(paths.baseSrc + "templates/**/*", series(templates, reloadBrowserSync));
    watch(paths.baseSrc + "js/**/*", series(javascript, reloadBrowserSync));
    watch(paths.vueSrc + "**/*.js", series(vueScripts, reloadBrowserSync));
    watch(paths.components + "**/*.vue", series(vue, reloadBrowserSync));
    watch(paths.baseSrc + "scss/**/*.scss", series(scss, reloadBrowserSync));
}

const entry = function () {
    return src(paths.vueSrc + "main.js").pipe(dest(paths.baseDist));
};

exports.default = series(
    html,
    plugins,
    parallel(locales, fonts, images, templates, vueScripts, javascript, scss, vue, entry),
    parallel(watchFiles, initBrowserSync)
);

exports.build = series(
    clean,
    html,
    plugins,
    parallel(locales, fonts, images, templates, vueScripts, javascript, scss, vue, entry)
);
