import axios from 'axios'

export const api = axios.create({
  baseURL: '/api', // Change this if your backend API is at a different URL
  // You can add headers or other config here
}) 