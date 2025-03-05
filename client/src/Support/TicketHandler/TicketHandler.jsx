import React, { useEffect, useState, } from "react";
import { useParams, Link } from 'react-router';
import "./TicketHandler.css";


const TicketHandler =() =>
{
  const {ticketId} = useParams();
  const [ticket, setTicket] = useState([]);
  const [categories, setCategories] = useState([]);
  const [ticketStatus, setTicketStatus] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("");
  
  useEffect(() => {
    fetch(`/api/categories`)
        .then(response => response.json())
        .then(data => {
          console.log('Fetched Categories:', data);
          setCategories(data);
        })
        .catch((error) => console.error("Error fetching categories", error));
  }, []);

  useEffect(() => {
    fetch(`/api/ticketstatus`)
        .then(response => response.json())
        .then(data => {
          console.log("Fetched status data from api:", data);
          setTicketStatus(data);
        })
        .catch((error) => console.error("Error fetching status", error));
  }, []);
  
  useEffect(() => { 
    fetch(`/api/tickets/${ticketId}`)
        .then(response => {
          if (!response.ok) {
            throw new Error("Network response was not ok");
          }
          return response.json(); // No need to use JSON.parse here
        })
        .then(data => {
          console.log("Fetched ticket data:", data);
          setTicket(data); // Set the ticket data directly
          
          const matchingCategory = categories.find(category => category.name.trim().toLowerCase() === data.categoryname.trim().toLowerCase());

          if (matchingCategory) {
            setSelectedCategory(matchingCategory.id);
          }
          
          const matchingStatus = ticketStatus.find(status => status.statusName === data.status);

          if (matchingStatus) {
            setSelectedStatus(matchingStatus.id);
          }
        })
        .catch((error) => console.error("Error fetching ticket", error));
  }, [ticketId, ticketStatus, categories]);

  const handleTicketStatusChange = async (e) => {
    const newStatusId = parseInt(e.target.value);
    setSelectedStatus(newStatusId);

    try {
      const response = await fetch(`/api/ticketstatus?ticket_id=${ticket.id}&status=${newStatusId}`, {
        method: "PUT",
        headers: {"Content-Type": "application/json",},
      });

      if (!response.ok) {
        throw new Error("Failed to update ticket Status");
      }

      alert("Ticket Status updated successfully!");
    } catch (error) {
      console.error("Error updating ticket:", error);
    }
  };

  const handleCategoryChange = async (e) => {
    const newCategoryId  = e.target.value;
    setSelectedCategory(newCategoryId);

    try {
      const response = await fetch(`/api/ticketscategory?ticket_id=${ticket.id}&category_id=${newCategoryId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
      });

      if (!response.ok) {
        throw new Error("Failed to update ticket Category");
      }

      alert("Ticket Category updated successfully!");
    } catch (error) {
      console.error("Error updating ticket:", error);
    }
  };
  
  if (!ticket) return <p>Loading ticket...</p>;
  
  return <main>
    <div key={"handler-div-" + ticket.id} className="handler-div">
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
            <textarea></textarea>
          </form>
        </div>
        <div className="ticket-status-product-div">
          <div className="ticket-status-div">
            <label>Ticket Status</label>
            <form onSubmit={(e) => e.preventDefault()}>
              <select
                  value={selectedStatus || ""}
                  onChange={handleTicketStatusChange}>
                {ticketStatus.map((status) => (
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
                      <option key={category.id} value={category.id}>  {/* Assuming 'category.name' is the name */}
                        {category.name}
                      </option>
                  ))}
                </select>
              </form>
            </div>
            <div className="ticket-product-div">
              <label>Product</label>
              <form>
                <select>
                  <option>Select Product</option>
                </select>
              </form>
            </div>
          </div>
        </div>
      </div>
      <div className="ticket-chat-div">
        <div className="chat-div">
          <label>Chat</label>
        </div>
        <div className="chat-response-div">
          <form>

          </form>
        </div>
        <button>Send</button>
      </div>
    </div>

  </main>
}

export default TicketHandler;