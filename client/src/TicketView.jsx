import React, { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import "./TicketView.css"

function BoxesContainer() {
    return (
        <div className="container">
            <div className="column">
                <div className="box">Active:</div>
                <div className="box">Inactive:</div>
                <div className="box">Resolved:</div>
            </div>
            <div className="box box4">Total Tickets:</div>
        </div>
    );
}


// Regular TicketView
export default function TicketView() {
    const [tickets, setTickets] = useState([]);
    const [searchparams] = useSearchParams();
    const view = searchparams.get("view");
    useEffect(() => {
        fetch("/api/tickets?view=" + view)
            .then((response) => response.json())
            .then((data) => {
                setTickets(data);
            })
            .catch((error) => console.error("Error fetching tickets:", error));
    }, [view]);

    // Rendering the table
    function TableItem(ticket) {
        return <tr key={"ticket-container-" + ticket.id}>
            <td>{ticket.date}</td>
            <td>{ticket.title}</td>
            <td>{ticket.categoryname}</td>
            <td>{ticket.email}</td>
            <td>{ticket.status}</td>
            <td><button className="resolve-button">Resolve Ticket</button></td>
        </tr>
    }
    return (
        <div className="ticket-container">
            <BoxesContainer />
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
                    {tickets.map(TableItem)}
                </tbody>
            </table>
        </div>
    );
};