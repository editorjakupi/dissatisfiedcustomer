import { StrictMode, useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { useParams } from 'react-router';
import '../../main.css';

export function Feedback() {

  const [rating, setRating] = useState(3);

  function GetRating() {
    const ratings = [];
    for (let index = 1; index <= 5; index++) {
      if (index <= rating) {
        ratings.push(<div className="star selected" id={index} onClick={(e) => setRating(e.target.id)}>★</div>);
      } else {
        ratings.push(<div className="star" id={index} onClick={(e) => setRating(e.target.id)}>☆</div>);
      }
    }
    return ratings;
  }


  return <div id='feedbackWindow'>
    <ul className='rating'>
      {GetRating()}
    </ul>
    <textarea></textarea>
    <div><button class="cancel button">Cancel</button><button class="send button" onClick={() => xdxdxd}>Send</button></div>
  </div>
}