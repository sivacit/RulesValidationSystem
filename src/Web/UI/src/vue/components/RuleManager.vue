<template>
  <div class="rule-manager">
    <!-- LEFT: Form -->
    <section class="form-section">
      <div class="top-row">
        <h2 class="title">Rule Manager</h2>
        <div class="buttons">
          <button class="btn add" @click="addRule">
            <span class="material-icons">add_circle</span> Add Rule
          </button>
          <button class="btn save" @click="saveRules" :disabled="rules.length === 0">
            <span class="material-icons">save</span> Save Rules
          </button>
        </div>
      </div>

      <div v-for="(rule, index) in rules" :key="index" class="rule-block">
        <!-- First Row -->
        <div class="row first-row">
          <div class="field field-medium">
            <label>Field Name</label>
            <input
              type="text"
              v-model="rule.fieldName"
              placeholder="Field Name"
            />
          </div>
          <div class="field field-large">
            <label>Expression</label>
            <textarea
              v-model="rule.expression"
              placeholder="Expression input 1.value > 10"
              @keydown="handleExpressionKeydown"
              ref="expressionAreas"
            ></textarea>
          </div>
        </div>

        <!-- Second Row -->
        <div class="row second-row">
          <div class="field field-medium">
            <label>Success Event</label>
            <input
              type="text"
              v-model="rule.successEvent"
              placeholder="Success Event"
            />
          </div>

          <div class="field field-medium">
            <label>Error Message</label>
            <input
              type="text"
              v-model="rule.errorMessage"
              placeholder="Error Message"
            />
          </div>

          <div class="field checkbox-field">
            <label><input type="checkbox" v-model="rule.enabled" /> Enabled</label>
          </div>

          <button class="delete-btn" @click="removeRule(index)" title="Delete">
            <span class="material-icons">delete</span>
          </button>
        </div>
      </div>
    </section>

    <!-- RIGHT: JSON Viewer -->
    <section class="json-section">
      <h2 class="title">Rules JSON</h2>
      <pre><code v-html="highlightedRules"></code></pre>
    </section>
  </div>
</template>

<script>
export default {
  name: "RuleManager",
  data() {
    return {
      rules: [
        {
          fieldName: "",
          expression: "",
          successEvent: "",
          errorMessage: "",
          enabled: false,
        },
      ],
    };
  },
  computed: {
    formattedRules() {
      return JSON.stringify(
        {
          WorkflowName: "Discount",
          Rules: this.rules,
        },
        null,
        2
      );
    },
    highlightedRules() {
      const json = this.formattedRules;
      const escaped = json
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
      return escaped
        .replace(
          /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(?=\s*:))/g,
          '<span class="json-key">$1</span>'
        )
        .replace(
          /(:\s*)("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*")/g,
          '$1<span class="json-string">$2</span>'
        )
        .replace(
          /(:\s*)(\b\d+(\.\d+)?\b)/g,
          '$1<span class="json-number">$2</span>'
        )
        .replace(
          /(:\s*)(\b(true|false|null)\b)/g,
          '$1<span class="json-boolean">$2</span>'
        );
    },
  },
  methods: {
    addRule() {
      this.rules.push({
        fieldName: "",
        expression: "",
        successEvent: "",
        errorMessage: "",
        enabled: false,
      });
      this.$nextTick(() => {
        // Optional: focus new expression textarea
      });
    },
    removeRule(index) {
      this.rules.splice(index, 1);
    },
    async saveRules() {
      try {
        const res = await fetch("http://localhost:8000/api/rules", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            WorkflowName: "Discount",
            Rules: this.rules,
          }),
        });
        if (!res.ok) throw new Error("Failed to save");
        alert("Rules saved!");
      } catch (err) {
        console.error(err);
        alert("Error saving rules");
      }
    },
    handleExpressionKeydown(event) {
      if (event.key === "Tab") {
        event.preventDefault();
        const textarea = event.target;
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const value = textarea.value;
        const spaces = "  "; // two spaces

        textarea.value = value.substring(0, start) + spaces + value.substring(end);
        textarea.selectionStart = textarea.selectionEnd = start + spaces.length;

        const index = Array.from(this.$refs.expressionAreas).indexOf(textarea);
        if (index !== -1) {
          this.rules[index].expression = textarea.value;
        }
      }
    },
  },
};
</script>

<style scoped>
@import url("https://fonts.googleapis.com/icon?family=Material+Icons");

.rule-manager {
  display: flex;
  gap: 30px;
  padding: 30px;
  font-family: Arial, sans-serif;
  background-color: #f9fafc;
  min-height: 100vh;
}

.form-section {
  flex: 1.2; /* Increased width */
  background: #fff;
  border-radius: 12px;
  padding: 25px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.json-section {
  flex: 0.6; /* Reduced width */
  background: #252f4a;
  color: #eee;
  border-radius: 12px;
  padding: 25px;
  overflow-y: auto;
}

.top-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.title {
  font-size: 22px;
  font-weight: bold;
}

.rule-block {
  border: 1px solid #d3d7e0;
  padding: 20px;
  border-radius: 10px;
  margin-bottom: 20px;
  background-color: #f5f8fc;
}

/* Rows */
.row {
  display: flex;
  gap: 15px;
  flex-wrap: nowrap;
  align-items: flex-end;
  margin-bottom: 15px;
}

.first-row {
  /* Field Name + Expression */
}

.second-row {
  /* Success Event + Error Message + Checkbox + Delete button */
  align-items: center;
}

/* Field widths */
.field {
  display: flex;
  flex-direction: column;
}

.field-medium {
  flex: 1 1 220px;
  min-width: 220px;
}

.field-large {
  flex: 2 1 400px;
  min-width: 320px;
}

.checkbox-field {
  display: flex;
  align-items: center;
  margin-top: 26px;
  flex-shrink: 0;
}

/* Inputs & Textareas */
input[type="text"] {
  width: 100%;
  padding: 10px 14px;
  font-size: 16px;
  border-radius: 8px;
  border: 1.5px solid #ccc;
  box-sizing: border-box;
  background-color: #fff;
  height: 40px;
}

textarea {
  width: 100%;
  height: 130px;
  padding: 12px 14px;
  border-radius: 8px;
  border: 1.5px solid #ccc;
  font-size: 15px;
  font-family: monospace;
  resize: both;
  background-color: #fff;
  box-sizing: border-box;
}

.field label {
  font-weight: 600;
  font-size: 14px;
  margin-bottom: 6px;
}

.checkbox-field label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  font-size: 14px;
}

.delete-btn {
  background: none;
  border: none;
  color: #e74c3c;
  font-size: 28px;
  cursor: pointer;
  padding: 0 8px;
  margin-left: auto;
  align-self: center;
}

.delete-btn:hover {
  color: #c0392b;
}

.buttons {
  display: flex;
  gap: 15px;
}

.btn {
  padding: 10px 20px;
  font-weight: 600;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  color: white;
  font-size: 14px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.12);
  user-select: none;
}

.add {
  background-color: #2f80ed;
}

.add:hover {
  background-color: #1c5cc5;
}

.save {
  background-color: #6a11cb;
}

.save:hover {
  background-color: #4b0ca8;
}

pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  font-size: 13px;
  line-height: 1.4;
  font-family: monospace;
}

.json-key {
  color: #9cdcfe;
}

.json-string {
  color: #ce9178;
}

.json-number {
  color: #b5cea8;
}

.json-boolean {
  color: #569cd6;
}
</style>
