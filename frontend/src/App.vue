<script setup>
import Navbar from './components/Navbar.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <div class="bg-white flex flex-col min-h-screen">
    <Navbar v-if="route.name !== 'login'" />
    <main :class="route.name !== 'login' ? 'pt-16 flex-1 flex flex-col' : 'flex-1 flex flex-col'">
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
