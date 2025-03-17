import React, { useState, useEffect } from "react";
import { useParams } from "react-router";
import "../../main.css";
import EmployeeChat from "./EmployeeChat";

// Komponent för att hantera ett kundärende, inklusive detaljer, status, kategori och meddelanden.
const TicketHandler = () => {
  const { ticketId } = useParams(); // Hämtar ticketId från URL:n.
  const [ticket, setTicket] = useState(null); // Håller information om det aktuella ärendet.
  const [categories, setCategories] = useState([]); // Håller tillgängliga kategorier.
  const [products, setProducts] = useState([]); // Håller tillgängliga produkter kopplade till företaget.
  const [ticketStatusList, setTicketStatusList] = useState([]); // Håller möjliga statusar för ärendet.
  const [selectedCategory, setSelectedCategory] = useState(""); // Vald kategori.
  const [selectedStatus, setSelectedStatus] = useState(""); // Vald status.
  const [selectedProduct, setSelectedProduct] = useState(""); // Vald produkt.
  const [messages, setMessages] = useState([]); // Håller meddelanden kopplade till ärendet.

  // Polling för att hämta nya meddelanden var 5:e sekund om ärendet laddats.
  useEffect(() => {
    const intervalId = setInterval(() => {
      if (ticket && ticket.id) {
        fetchMessages(ticket.id);
      }
    }, 5000);
    return () => clearInterval(intervalId); // Rensar intervallet vid avmontering.
  }, [ticket]);

  // Hämtar alla tillgängliga kategorier från servern.
  useEffect(() => {
    fetch(`/api/categories`)
      .then(response => response.json())
      .then(data => {
        console.log("Fetched Categories:", data);
        setCategories(data); // Sparar kategorier i state.
      })
      .catch(error => console.error("Error fetching categories", error)); // Loggar fel om hämtning misslyckas.
  }, []);

  // Hämtar alla möjliga statusar för ett ärende.
  useEffect(() => {
    fetch(`/api/ticketstatus`)
      .then(response => response.json())
      .then(data => {
        console.log("Fetched ticket status list:", data);
        setTicketStatusList(data); // Sparar statuslista i state.
      })
      .catch(error => console.error("Error fetching ticket status", error));
  }, []);

  // Hämtar ärendedata och tillhörande produkter baserat på ärendets företags-ID.
  useEffect(() => {
    if (!ticketId) return; // Om ticketId inte finns, avbryt.
    fetch(`/api/tickets/${ticketId}`)
      .then(response => {
        if (!response.ok) throw new Error("Network response was not ok");
        return response.json();
      })
      .then(data => {
        console.log("Fetched ticket data:", data);
        setTicket(data); // Sparar ärendedata.
        if (data && data.id) {
          fetchMessages(data.id); // Hämtar meddelanden för ärendet.
          if (data.companyId) {
            fetch(`/api/products/${data.companyId}`) // Hämtar produkter för företaget kopplat till ärendet.
              .then(response => response.json())
              .then(productData => {
                console.log("Fetched product data:", productData);
                setProducts(productData);
              })
              .catch(error => console.error("Error fetching products", error));
          } else {
            console.warn("Company ID is undefined");
            setProducts([]); // Återställ produkter om företags-ID saknas.
          }
          // Matchar den nuvarande kategorin om den finns i kategorilistan.
          if (data.categoryname && categories.length > 0) {
            const matchingCategory = categories.find(category =>
              category.name.trim().toLowerCase() === data.categoryname.trim().toLowerCase()
            );
            if (matchingCategory) {
              setSelectedCategory(matchingCategory.id);
            }
          }
          // Matchar den nuvarande statusen om den finns i statuslistan.
          if (data.status && ticketStatusList.length > 0) {
            const matchingStatus = ticketStatusList.find(status =>
              status.statusName === data.status
            );
            if (matchingStatus) {
              setSelectedStatus(matchingStatus.id);
            }
          }
        }
      })
      .catch(error => console.error("Error fetching ticket", error)); // Loggar fel om hämtningen misslyckas.
  }, [ticketId, categories, ticketStatusList]);

  // Uppdaterar ärendets status lokalt baserat på vald status i dropdown-menyn.
  useEffect(() => {
    if (!ticket) return;
    setTicket(prevTicket => ({
      ...prevTicket,
      status: ticketStatusList.find(status => status.id === selectedStatus)?.statusName || prevTicket.status
    }));
  }, [selectedStatus, ticketStatusList]);

  // Hämtar alla meddelanden kopplade till ett ärende-ID.
  const fetchMessages = (ticketId) => {
    fetch(`/api/tickets/${ticketId}/messages`, {
      method: "GET",
      credentials: "include" // Inkluderar autentisering.
    })
      .then(response => {
        if (response.status === 204) return []; // Returnerar tom lista om inga meddelanden finns.
        return response.json().catch(() => []); // Försöker tolka svar även om det är tomt.
      })
      .then(data => {
        console.log("Fetched messages:", data);
        setMessages(data); // Uppdaterar meddelandelistan i state.
      })
      .catch(err => console.error("Error fetching messages:", err)); // Loggar eventuella fel.
  };

  // Uppdaterar ärendets status via API.
  const handleTicketStatusChange = async (e) => {
    const newStatusId = parseInt(e.target.value);
    setSelectedStatus(newStatusId); // Uppdaterar vald status i state.
    try {
      const response = await fetch(`/api/ticketstatus?ticketId=${ticket.id}&status=${newStatusId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) throw new Error("Failed to update ticket Status");
      alert("Ticket Status updated successfully!"); // Visar framgångsmeddelande.
    } catch (error) {
      console.error("Error updating ticket status:", error); // Loggar eventuellt fel.
    }
  };

  // Uppdaterar ärendets kategori via API.
  const handleCategoryChange = async (e) => {
    const newCategoryId = e.target.value;
    setSelectedCategory(newCategoryId); // Uppdaterar vald kategori i state.
    try {
      const response = await fetch(`/api/ticketscategory?ticket_id=${ticket.id}&category_id=${newCategoryId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) throw new Error("Failed to update ticket Category");
      alert("Ticket Category updated successfully!");
    } catch (error) {
      console.error("Error updating ticket category:", error); // Loggar fel om uppdatering misslyckas.
    }
  };

  // Uppdaterar ärendets produkt via API.
  const handleProductChange = async (e) => {
    const newProductId = e.target.value;
    setSelectedProduct(newProductId); // Uppdaterar vald produkt i state.
    try {
      const response = await fetch(`/api/ticketsproduct?ticket_id=${ticket.id}&product_id=${newProductId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) throw new Error("Failed to update ticket Product");
      alert("Ticket Product updated successfully!");
    } catch (error) {
      console.error("Error updating ticket product:", error);
    }
  };

  if (!ticket) return <p>Loading ticket...</p>; // Visar laddningsmeddelande om ärendet inte är hämtat.

  return (
    <main className="ticket-handler-container">
      <div key={`handler-div-${ticket.id}`} className="handler-div">
        {/* Ärendedetaljer */}
        <div className="ticket-handle-information-div">
          <div className="ticket-title-information-div">
            <label>Title</label>
            <label className="ticket-title-label">{ticket.title} #{ticket.caseNumber}</label>
          </div>
          <div className="ticket-description-information-div">
            <label>Description</label>
            <label className="ticket-description-label">{ticket.description}</label>
          </div>
          <div className="ticket-notes">
            <label>Internal notes</label>
            <form>
              <textarea placeholder="Write internal notes here..."></textarea>
            </form>
          </div>
          {/* Ändra status, kategori och produkt */}
          <div className="ticket-status-product-div">
            <div className="ticket-status-div">
              <label>Ticket Status</label>
              <form onSubmit={(e) => e.preventDefault()}>
                <select value={selectedStatus || ""} onChange={handleTicketStatusChange}>
                  {ticketStatusList.map((status) => (
                    <option key={status.id} value={status.id}>
                      {status.statusName}
                    </option>
                  ))}
                </select>
              </form>
            </div>
            <div className="ticket-product-category-div">
              <div className="ticket-category-div">
                <label>Category</label>
                <form onSubmit={(e) => e.preventDefault()}>
                  <select value={selectedCategory || ""} onChange={handleCategoryChange}>
                    {categories.map((category) => (
                      <option key={category.id} value={category.id}>
                        {category.name}
                      </option>
                    ))}
                  </select>
                </form>
              </div>
              <div className="ticket-product-div">
                <label>Product</label>
                <form onSubmit={(e) => e.preventDefault()}>
                  <select value={selectedProduct || ""} onChange={handleProductChange}>
                    {products.map((product) => (
                      <option key={product.id} value={product.id}>
                        {product.name}
                      </option>
                    ))}
                  </select>
                </form>
              </div>
            </div>
          </div>
        </div>
        {/* Chattsektion */}
        <div className="ticket-chat-div">
          <div className="chat-div">
            <label>Chat</label>
          </div>
          <ul className="messages-list">
            {messages && messages.length > 0 ? (
              messages.map((msg, index) => {
                const senderType = msg.senderType?.trim().toLowerCase() || ""; // Avgör om avsändaren är kund eller support.
                const isCustomerMessage = senderType === "customer"; // Kontroll om det är kundmeddelande.
                const prefix = isCustomerMessage ? "Customer: " : "Customer Service: "; // Prefix beroende på avsändartyp.
                const messageClass = isCustomerMessage ? "customer-message" : "employee-message"; // Klass för att styla meddelandet.
                return (
                  <li key={index} className={`message-item ${messageClass}`}>
                    <p>{prefix}{msg.content}</p>
                  </li>
                );
              })
            ) : (
              <li className="message-item">No messages yet.</li> // Visar om inga meddelanden finns.
            )}
          </ul>
          {/* EmployeeChat-komponent för att lägga till meddelanden */}
          <EmployeeChat ticket={ticket} fetchMessages={fetchMessages} />
        </div>
      </div>
    </main>
  );
};

export default TicketHandler;
