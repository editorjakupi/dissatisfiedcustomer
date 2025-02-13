import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route } from 'react-router'
import AccountInformation from './account.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App/>
  </StrictMode>,
)

function App()
{
  return <BrowserRouter>
  <Routes>
    <Route path='account' element={ <AccountInformation/> } />
  </Routes>
  </BrowserRouter>
}