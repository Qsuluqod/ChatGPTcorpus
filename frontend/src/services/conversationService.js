import { api } from './api'

export async function getConversation(conversationId) {
  try {
    const response = await api.get(`/conversations/${conversationId}`)
    return {
      success: true,
      conversation: response.data
    }
  } catch (error) {
    console.error('Error fetching conversation:', error)
    return {
      success: false,
      error: 'Failed to load conversation'
    }
  }
} 