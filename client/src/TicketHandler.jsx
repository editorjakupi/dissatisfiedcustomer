import { useEffect, useState, } from "react";
import { useParams } from "react-router";
import "./TicketHandler.css";


function TicketHandler()
{
  const {ticketId} = useParams();
  const [ticket, setTicket] = useState([]);
  const [categories, setCategories] = useState([]);

  useEffect(() => {
    fetch(`/api/tickets/${ticketId}`)
    .then(response => response.json())
    .then(data => {
      console.log("Fetched ticket data from api: ", data)
      setTicket(data);
    })
    .catch((error) => console.error("Error fetching ticket", error));
  }, [ticketId]);

  useEffect(() => {
    fetch(`/api/categories`)
    .then(response => response.json())
    .then(data => {
      console.log("Fetched category data from api: ", data)
      setCategories(data);
    })
    .catch((error) => console.error("Error fetching categories", error));
  }, []);

  function handlerUI(ticket) {
    return <div key={"handler-div-" + ticket.id}>
      <div className="ticket-handle-information-div">
      <div className="ticket-title-information-div">
        <label>Title</label>
        <label>{ticket.title}</label>
      </div>
      <div className="ticket-description-information-div">
        <label>Description</label>
        <p>{ticket.description}</p>
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
          <select>
            <option>Unresolved</option>
            <option>Resolved</option>
          </select>
        </div>
        <div className="ticket-product-category-div">
        <div className="ticket-category-div">
        <label>Category</label>
          <form>
            <select defaultValue={ticket.categoryname}>
              {categories.map((categories) => (
                <option key={categories.name} value={categories.name}>
                  {categories.name}
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
      </div>
      <div className="chat-response-div">
        <form>
          <textarea></textarea>
        </form>
      </div>
      <button>Send</button>
    </div>
    </div>
  }

  return <main>
    {ticket.map(handlerUI)}
  </main>
}

export default TicketHandler;