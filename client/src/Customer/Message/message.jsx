// src/components/Message.jsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { useParams } from 'react-router';
// import '../../message.css';

export function Message() {
  const params = useParams();

  return (
    <main>
      <ul>
        <div><h3>Title</h3><input className="title" placeholder='Title for your problem' /></div>
        <div><h3>Email</h3><input className="email" placeholder='example@mail.com' /></div>
        <div><h3>Message</h3><textarea className="message" placeholder='Write your problem in detail here' /></div>
        <div><button className="cancel button">Cancel</button><button className="submit button" onClick={() => submitMessage(params.id)}>Submit</button></div>
      </ul>
    </main>
  );
}

function submitMessage(companyId) {
  console.log("CompanyID:", companyId);
  fetch("http://localhost:5000/api/messages", {
    method: "POST",
    credentials: 'include',  // Lägg till denna rad här så att cookien skickas med
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      "Email": document.querySelector(".email").value,
      "Name": document.querySelector(".title").value,
      "Content": document.querySelector(".message").value,
      "CompanyID": parseInt(companyId)
    })
  })
  .then(response => {
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    return response.json();
  })
  .then(data => console.log(data))
  .catch(error => console.error('Error:', error));
}


