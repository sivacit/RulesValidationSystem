<template>
  <div id="app">
    <h1>Vue 3 + Gulp</h1>
    <p>This component is compiled from a .vue file.</p>
  </div>
</template>

<script>
export default {
  name: 'App',
  mounted() {
    console.log("App mounted");
  }
};
</script>

<style scoped>
#app {
  font-family: Arial, sans-serif;
  padding: 2rem;
  text-align: center;
  background: #f2f4f8;
}
</style>
