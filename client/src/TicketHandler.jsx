import { useEffect, useState, use, useParams } from "react"
import "./TicketHandler.css"


function TicketHandler()
{
  const {caseId} = useParams();
  const [ticket, setTicket] = useState([]);

  useEffect(() => {
    fetch(`/api/tickets/${caseId}`)
    .then(response => response.json)
    .then(data => {setTicket(data)})
  });

  
  
  // const [searchoarans] = useParams();
  // const view = searchparams.get("view");

  return <main>
    <div className="ticket-handle-information-div">
      <div className="ticket-title-information-div">
      </div>
      <div className="ticket-description-information-div">
      </div>
      <div className="ticket-notes">
      </div>
      <div className="ticket-status-product-div">
        <div className="ticket-status-div">
        </div>
        <div className="ticket-product-category-div">
          <div className="ticket-product-div">
          </div>
          <div className="ticket-category-div">
          </div>
        </div>
      </div>
    </div>
    <div className="ticket-chat-div">
      <button>Send</button>
    </div>
  </main>
}

export default TicketHandler;