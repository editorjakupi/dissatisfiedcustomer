import React, { useEffect, useState } from "react";
import "./TicketView.css"

const TicketView = () => {
    const [tickets, setTickets] = useState([]);

    useEffect(() => {
        fetch("/api/tickets")
            .then((response) => response.json())
            .then((data) => {console.log("Fetched Tickets:", data)
            setTickets(data);
        })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, []);

    return (
        <div className="ticket-container">
            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Title</th>
                        <th>Category</th>
                        <th>E-Mail</th>
                        <th>Status</th>
                        <th>Mark As Resolved</th>
                    </tr>
                </thead>
                <tbody>
                    {tickets.map((ticket) => (
                        <tr key={ticket.id}>
                            <td>{ticket.date}</td> {/* Format YYYY-MM-DD */}
                            <td>{ticket.title}</td>
                            <td>{ticket.categoryname}</td>
                            <td>{ticket.email}</td>
                            <td>{ticket.status_name}</td>
                            <td>
                                <button className="resolve-button">Resolve Ticket</button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default TicketView;