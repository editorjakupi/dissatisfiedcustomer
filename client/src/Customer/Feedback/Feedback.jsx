import { StrictMode, useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { useParams } from 'react-router';

export function Feedback() {

  const [rating, setRating] = useState(3);

  function GetRating() {
    const ratings = [];
    for (let index = 1; index <= 5; index++) {
      if (index <= rating) {
        ratings.push(<div>★</div>);
      } else {
        ratings.push(<div>☆</div>);
      }
    }
    return ratings;
  }


  return <div>
    <ul className='rating'>
      {GetRating()}
    </ul>
    <div>
      <button onClick={() => setRating(rating + 1)}>+</button>
      <button onClick={() => setRating(rating - 1)}>-</button>
    </div>
    <textarea></textarea>
    <div><button class="cancel button">Cancel</button><button class="send button" onClick={() => xdxdxd}>Send</button></div>
  </div>
}