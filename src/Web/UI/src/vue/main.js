import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import App from './components/App.js'; // <-- Gulp compiles App.vue to this

createApp(App).mount('#app');
