import { createRouter, createWebHistory } from 'vue-router'
import SearchView from '../views/SearchView.vue'
import ContributeView from '../views/ContributeView.vue'
import CorpusInfoView from '../views/CorpusInfoView.vue'
import ConversationView from '../views/ConversationView.vue'
import LoginView from '../views/LoginView.vue'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/search',
      name: 'search',
      component: SearchView
    },
    {
      path: '/contribute',
      name: 'contribute',
      component: ContributeView
    },
    {
      path: '/corpus-info',
      name: 'corpus-info',
      component: CorpusInfoView
    },
    {
      path: '/conversation/:conversationId/:messageId',
      name: 'conversation',
      component: ConversationView
    }
  ]
})

router.beforeEach((to, from, next) => {
  const required = import.meta.env.VITE_ACCESS_PASSPHRASE
  if (required && to.name === 'search') {
    const authenticated = localStorage.getItem('authenticated') === 'true'
    if (!authenticated) {
      return next({ name: 'login' })
    }
  }
  next()
})

export default router
