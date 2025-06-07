<script setup>
import Navbar from './components/Navbar.vue'
</script>

<template>
  <div class="bg-white flex flex-col">
    <Navbar />
    <main class="pt-16 flex-1 flex flex-col">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>
