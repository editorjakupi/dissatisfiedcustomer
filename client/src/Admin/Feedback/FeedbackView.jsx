import React, {useEffect, useState} from "react";

const FeedbackView =({user}) => {
    const [tickets, setTickets] = useState([]);
    const [selectedTicket, setSelectedTicket] = useState(null);
    
    useEffect(() => {
        fetch(`/api/tickets/feedback?companyId=${user.companyId}`)
            .then((res) => res.json())
            .then((data) => {
                console.log("Ticket Feedback Response:", data); // Log the API response
                setTickets(data)
            })
            .catch((error) => console.error("Error fetching ticket feedback:", error));
    }, []);

    const handleTicketClick = (ticket) => {
        setSelectedTicket(ticket.id === selectedTicket?.id ? null : ticket);
    };
    
    return (
        <div>
            <h2>Ticket Feedback</h2>
            {tickets.length > 0 ? (
                <ul>
                    {tickets.map((ticket) => (
                        <li key={ticket.id}>
                            <strong
                                onClick={() => handleTicketClick(ticket)}
                                style={{ cursor: "pointer", color: "blue" }}
                            >
                                {ticket.title} {new Date(ticket.date).toLocaleDateString()}
                            </strong>
                            {selectedTicket?.id === ticket.id && (
                                <>
                                    <p>Ticket Title: {ticket.title}</p>
                                    <p>Date: {new Date(ticket.date).toLocaleDateString()}</p>
                                    {ticket.comment ? (
                                        <>
                                            <p>Customer: {ticket.userEmail}</p>
                                            <p>Employee Name: {ticket.employeeName}</p>
                                            <p>Rating: {ticket.rating}/5</p>
                                            <p>Comment: {ticket.comment}</p>
                                            <p></p>
                                        </>
                                    ) : (
                                        <p>No feedback yet.</p>
                                    )}
                                </>
                            )}
                        </li>
                    ))}
                </ul>
            ) : (
                <p>No tickets found for this company.</p>
            )}
        </div>
    );
}

export default FeedbackView;