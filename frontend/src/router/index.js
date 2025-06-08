import { createRouter, createWebHistory } from 'vue-router'
import SearchView from '../views/SearchView.vue'
import ContributeView from '../views/ContributeView.vue'
import CorpusInfoView from '../views/CorpusInfoView.vue'
import ConversationView from '../views/ConversationView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/search'
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

export default router 