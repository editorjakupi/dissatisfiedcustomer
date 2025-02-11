import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Message } from './message'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <Message />
  </StrictMode>,
)
