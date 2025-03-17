// src/components/Message.jsx
import { StrictMode, useState, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import { useParams } from 'react-router';
import './QuestDemo.css'
import './NvidiaDemo.css'

export function DemoPage() {
  const params = useParams();

  const [catAndProd, setCatAndProd] = useState([]);

  let company = "";

  useEffect(() => {
  fetch("/api/demoinfo/" + params.id)
  .then((response) => response.json())
  .then((data) => {
    console.log("Fetched categories and products:", data)
    setCatAndProd(data);
  })
  .catch((error) => console.error("Error fetching categories and products:", error))
}, []);

if(params.id == 1){
  company = "quest";
}else if(params.id == 2){
  company = "nvidia";
}

  return (
    <main className={company}>
      <div className={company + ' topbar'}>
      <img className={company + ' logo'}/>
      </div>
      <ul className={company + " entries"}>
        <div><h3>Title</h3><input className={company + " title"} placeholder='Title for your problem' /></div>
        <div><h3>Email</h3><input className={company + " email"} placeholder='example@mail.com' /></div>
        <div><h3>Category</h3><select class={company + " category"} defaultValue={null}>
      <option key={null} value={null} hidden>Choose Category</option>
        {catAndProd.categories?.map(category => 
          <option key={category.id} value={category.id}>{category.name}</option>)}
      </select></div>
      <div><h3>Product/Service</h3><select class={company + " product"} defaultValue={null}>
      <option key={null} value={null} hidden>Choose Product/Service</option>
        {catAndProd.products?.map(product => 
          <option key={product.id} value={product.id}>{product.name}</option>)}
      </select></div>
        <div><h3>Message</h3><textarea className={company + " message"} placeholder='Write your problem in detail here' /></div>
        <div className={company + " buttons"}><button className={company + " cancel button"}>Cancel</button><button className={company + " submit button"} onClick={() => submitMessage(params.id)}>Submit</button></div>
      </ul>
    </main>
  );
}

function submitMessage(companyId) {
  console.log("CompanyID:", companyId);
  fetch("/api/messages", {
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
      "CompanyID": parseInt(companyId),
      "ProductID": document.querySelector(".product").value,
      "CategoryID": document.querySelector(".category").value
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