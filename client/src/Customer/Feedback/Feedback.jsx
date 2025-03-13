import { StrictMode, useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { useParams } from 'react-router';
import '../../main.css';

export function Feedback({ caseId }) 
{
  const [rating, setRating] = useState(3);

  //☆
  function GetRating() {
    const ratings = [];
    for (let index = 1; index <= 5; index++) {
      if (index <= rating) {
        ratings.push(<div className="star selected" id={index} onClick={(e) => setRating(e.target.id)}>★</div>);
      } else {
        ratings.push(<div className="star" id={index} onClick={(e) => setRating(e.target.id)}>★</div>);
      }
    }
    return ratings;
  }


  return <div id='feedbackWindow'>
    <ul className='rating'>
      {GetRating()}
    </ul>
    <textarea className='comment'></textarea>
    <div><button class="cancel button">Cancel</button><button class="send button" onClick={() => SendFeedback(caseId, rating)}>Send</button></div>
  </div>
}

function SendFeedback(id, rating)
{
  console.log("Sending feedback to caseId: " + id);

  fetch("/api/feedback", {
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    },
    method: "POST",
    body: JSON.stringify({
      "ticket_id": id,
      "rating": rating,
      "comment": document.querySelector(".comment").value
    })
  })
    .then(response => response.json())  // Parsa JSON responsen från backenden
    .then(data => console.log(data));     // Logga responsen
}