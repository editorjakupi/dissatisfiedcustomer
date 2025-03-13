import React, { useState, useEffect } from "react";
import { useParams } from "react-router";
import "../../main.css";
import EmployeeChat from "./EmployeeChat";

const TicketHandler = () => {
  const { ticketId } = useParams();
  const [ticket, setTicket] = useState(null);
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [ticketStatusList, setTicketStatusList] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("");
  const [selectedProduct, setSelectedProduct] = useState("");
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    const intervalId = setInterval(() => {
      if (ticket && ticket.id) {
        fetchMessages(ticket.id);
      }
    }, 5000);
    return () => clearInterval(intervalId);
  }, [ticket]);

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
        console.log("Fetched ticket status list:", data);
        setTicketStatusList(data);
      })
      .catch((error) => console.error("Error fetching ticket status", error));
  }, []);

  useEffect(() => {
    if (!ticketId) return;
    fetch(`/api/tickets/${ticketId}`)
      .then(response => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        return response.json();
      })
      .then(data => {
        console.log("Fetched ticket data:", data);
        setTicket(data);
        if (data && data.id) {
          fetchMessages(data.id);
          if (data.company_id) {
            fetch(`/api/products/${data.company_id}`)
              .then(response => response.json())
              .then(productData => {
                console.log("Fetched product data:", productData);
                setProducts(productData);
              })
              .catch((error) => console.error("Error fetching products", error));
          } else {
            console.warn("Company ID is undefined");
            setProducts([]);
          }
          if (data.categoryname && categories.length > 0) {
            const matchingCategory = categories.find(category =>
              category.name.trim().toLowerCase() === data.categoryname.trim().toLowerCase()
            );
            if (matchingCategory) {
              setSelectedCategory(matchingCategory.id);
            }
          }
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
      .catch((error) => console.error("Error fetching ticket", error));
  }, [ticketId, categories, ticketStatusList]);

  useEffect(() => {
    if (!ticket) return;
    setTicket((prevTicket) => ({
      ...prevTicket,
      status: ticketStatusList.find((status) => status.id === selectedStatus)?.statusName || prevTicket.status
    }));
  }, [selectedStatus, ticketStatusList]);

  const fetchMessages = (ticketId) => {
    fetch(`/api/tickets/${ticketId}/messages`, {
      method: "GET",
      credentials: "include"
    })
      .then(response => {
        if (response.status === 204) {
          return [];
        }
        return response.json().catch(() => []);
      })
      .then(data => {
        console.log("Fetched messages:", data);
        setMessages(data);
      })
      .catch(err => {
        console.error("Error fetching messages:", err);
      });
  };

  const handleTicketStatusChange = async (e) => {
    const newStatusId = parseInt(e.target.value);
    setSelectedStatus(newStatusId);
    try {
      const response = await fetch(`/api/ticketstatus?ticketId=${ticket.id}&status=${newStatusId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) {
        throw new Error("Failed to update ticket Status");
      }
      alert("Ticket Status updated successfully!");
    } catch (error) {
      console.error("Error updating ticket status:", error);
    }
  };

  const handleCategoryChange = async (e) => {
    const newCategoryId = e.target.value;
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
      console.error("Error updating ticket category:", error);
    }
  };

  const handleProductChange = async (e) => {
    const newProductId = e.target.value;
    setSelectedProduct(newProductId);
    try {
      const response = await fetch(`/api/ticketsproduct?ticket_id=${ticket.id}&product_id=${newProductId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) {
        throw new Error("Failed to update ticket Product");
      }
      alert("Ticket Product updated successfully!");
    } catch (error) {
      console.error("Error updating ticket product:", error);
    }
  };

  if (!ticket) return <p>Loading ticket...</p>;

  return (
    <main className="ticket-handler-container">
      <div key={`handler-div-${ticket.id}`} className="handler-div">
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
        <div className="ticket-chat-div">
          <div className="chat-div">
            <label>Chat</label>
          </div>
          <ul className="messages-list">
            {messages && messages.length > 0 ? (
              messages.map((msg, index) => {
                const senderType = msg.senderType?.trim().toLowerCase() || "";
                const isCustomerMessage = senderType === "customer";
                const prefix = isCustomerMessage ? "Customer: " : "Customer Service: ";
                const messageClass = isCustomerMessage ? "customer-message" : "employee-message";
                return (
                  <li key={index} className={`message-item ${messageClass}`}>
                    <p>{prefix}{msg.content}</p>
                  </li>
                );
              })
            ) : (
              <li className="message-item">No messages yet.</li>
            )}
          </ul>
          <EmployeeChat ticket={ticket} fetchMessages={fetchMessages} />
        </div>
      </div>
    </main>
  );
};

export default TicketHandler;
