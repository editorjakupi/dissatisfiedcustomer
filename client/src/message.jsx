import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { useParams } from 'react-router'
import './message.css'

export function Message() {
  const params = useParams();

  return <main>
    <ul>
      <div><h3>Title</h3><input class="title" placeholder='Title for your problem' /></div>
      <div><h3>Email</h3><input class="email" placeholder='example@mail.com' /></div>
      <div><h3>Message</h3><textarea class="message" placeholder='Write your problem in detail here' /></div>
      <div><button class="cancel button">Cancel</button><button class="submit button" onClick={() => submitMessage(params.id)}>Submit</button></div>
    </ul>
  </main>
}

function submitMessage(companyId) {
  console.log(companyId);
  fetch("/api/messages", {
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    },
    method: "POST",
    body: JSON.stringify({
      "Email": document.querySelector(".email").value,
      "Name": document.querySelector(".title").value,
      "Content": document.querySelector(".message").value,
      "CompanyID": companyId
    })
  })
    .then(response => response.json())  // Parsa JSON responsen från backenden
    .then(data => console.log(data));     // Logga responsen
}
