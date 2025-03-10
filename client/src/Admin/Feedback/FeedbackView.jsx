import React, {useEffect, useState} from "react";

const FeedbackView =({user}) => {
    const [tickets, setTickets] = useState([]);

    useEffect(() => {
        fetch(`/api/tickets/feedback?companyId=${user.companyId}`)
            .then((res) => res.json())
            .then((data) => {
                console.log("Ticket Feedback Response:", data); // Log the API response
                setTickets(data)
            })
            .catch((error) => console.error("Error fetching ticket feedback:", error));
    }, []);

    return (
        <div>
            <h2>Ticket Feedback</h2>
            {tickets.length > 0 ? (
                <ul>
                    {tickets.map((ticket) => (
                        <li key={ticket.id}>
                            <strong>{ticket.title}</strong> <br />
                            {ticket.comment ? (
                                <>
                                    <p>Customer: {ticket.userEmail}</p>
                                    <p>Employee Name: {ticket.employeeName}</p>
                                    <p>Rating: {ticket.rating}/5</p>
                                    <p>Comment: {ticket.comment}</p>
                                    <p>Date: {new Date(ticket.date).toLocaleDateString()}</p>
                                </>
                            ) : (
                                <p>No feedback yet.</p>
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