// src/SessionTimer.jsx
import React, { useState, useEffect } from 'react';

// Komponent som visar en nedräkning för sessionens utgångstid.
const SessionTimer = ({ durationInSeconds }) => {
  const [timeLeft, setTimeLeft] = useState(durationInSeconds); // Håller antalet sekunder kvar innan sessionen går ut.

  // Effekt för att starta en nedräkning som uppdaterar tiden varje sekund.
  useEffect(() => {
    if (timeLeft <= 0) return; // Avbryter om tiden har gått ut.
    const intervalId = setInterval(() => {
      setTimeLeft(prev => prev - 1); // Minskar tiden med en sekund.
    }, 1000);
    return () => clearInterval(intervalId); // Rensar intervallet när komponenten avmonteras.
  }, [timeLeft]);

  // Konverterar kvarvarande tid till formatet mm:ss.
  const minutes = String(Math.floor(timeLeft / 60)).padStart(2, '0'); // Beräknar minuter och fyller med nollor.
  const seconds = String(timeLeft % 60).padStart(2, '0'); // Beräknar sekunder och fyller med nollor.

  // Renderar nedräkningstexten.
  return (
    <div className="session-timer">
      Session expires in: {minutes}:{seconds}
    </div>
  );
};

export default SessionTimer;
