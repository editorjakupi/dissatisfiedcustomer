import React, { useState } from "react";
import "../../main.css";

const NewEmployee = ({ user, setUser }) => {
    const [formData, setFormData] = useState({
        name: "",
        email: "",
        password: "",
        phonenumber: "",
        companyId: "",
    });

    const [message, setMessage] = useState("");
    const [employees, setEmployees] = useState([]);
    const [selectedEmployee, setSelectedEmployee] = useState(null);
    const [searchId, setSearchId] = useState("");

    const handleChange = (e) => {
        setFormData({...formData, [e.target.name]: e.target.value});
    };

    const handleSearch = () => {
        if (!searchId.trim()) return;

        fetch(`/api/employee/${searchId}`)
            .then((res) => res.json())
            .then((data) => {
                if (data) {
                    setEmployees([data]);
                    setSelectedEmployee(data);
                    setFormData(data); // Populate form with selected data
                }
            })
            .catch(() => {
                setEmployees([]);
                setSelectedEmployee(null);
            });
    };

    const handleShowAll = () => {
        fetch(`/api/employee/${user.companyId}`)
            .then((res) => res.json())
            .then((data) => {
                setEmployees(data);
                setSelectedEmployee(null);
                setSearchId("");
            })
            .catch(console.error);
    };

    const handleDelete = () => {
        if (!selectedEmployee) return;

        fetch(`/api/employees/${selectedEmployee.id}`, {
            method: 'DELETE',
        })
            .then((res) => {
                if (res.ok) {
                    setEmployees((prev) => prev.filter(emp => emp.id !== selectedEmployee.id));
                    setSelectedEmployee(null);
                    alert("Employee deleted successfully!");
                } else {
                    console.error("Error deleting Employee.");
                    setFormData({name: "", email: "", password: "", phonenumber: "", companyId: ""});
                }
            })
            .catch(console.error);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage("");

        if (selectedEmployee) {
            // Update existing employee
            try {
                const response = await fetch(`/api/employees/${selectedEmployee.id}`, {
                    method: "PUT",
                    headers: {"Content-Type": "application/json"},
                    body: JSON.stringify(formData),
                });

                if (!response.ok) throw new Error("Failed to update employee");

                setMessage("Employee updated successfully");
                setEmployees((prev) => prev.map(emp => emp.id === selectedEmployee.id ? {...emp, ...formData} : emp));
            } catch (error) {
                setMessage(error.message);
            }
        } else {
            // Create new employee
            try {
                const userResponse = await fetch("/api/users", {
                    method: "POST",
                    headers: {"Content-Type": "application/json"},
                    body: JSON.stringify({
                        name: formData.name,
                        email: formData.email,
                        password: formData.password,
                        phonenumber: formData.phonenumber,
                    }),
                });

                if (!userResponse.ok) throw new Error("Failed to create user");

                const userId = await userResponse.json();

                const employeeData = {userId, companyId: parseInt(user.companyId, 10)};

                const employeeResponse = await fetch("/api/employees", {
                    method: "POST",
                    headers: {"Content-Type": "application/json"},
                    body: JSON.stringify(employeeData),
                });

                if (!employeeResponse.ok) throw new Error("Failed to add employee");

            if (!employeeResponse.ok) throw new Error(employeeResponseText || "Failed to add employee");

            setMessage("User and Employee created successfully ID: " + userId);
            alert("Employee created successfully!");
        } catch (error) {
                console.error(error);
                setMessage(error.message);
                setMessage("User and Employee created successfully ID: " + userId);
                handleShowAll();
            }
        }
    };

    const handleClearSelection = () => {
        setSelectedEmployee(null); // Reset selected employee
        setFormData({
            name: "",
            email: "",
            password: "",
            phonenumber: "",
            companyId: "",
        });
        setMessage(null);
    };

    return (
        <main>
            <div className="user-container">
                <div className="search-container">
                    <input type="text" placeholder="Enter Employee ID" value={searchId}
                           onChange={(e) => setSearchId(e.target.value)} className="search-input"/>
                    <div className="button-container">
                        <button onClick={handleSearch} className="search-button">Search</button>
                        <button onClick={handleShowAll} className="show-all-button">Show All</button>
                    </div>
                </div>

                <div className="main-container">
                    <div className="content-wrapper">
                        <div className="users-list">
                            {employees.length > 0 ? (
                                employees.map((employee) => (
                                    <div key={employee.id} className="user-item"
                                         onClick={() => {
                                             setSelectedEmployee(employee);
                                             setFormData(employee);
                                         }}>
                                        {employee.name}
                                    </div>
                                ))
                            ) : <p>No Employees found.</p>}
                        </div>

                        <div className="user-details">
                            {selectedEmployee ? (
                                <div className="user-card">
                                    <h2>{selectedEmployee.name}</h2>
                                    <p><strong>Name:</strong> {selectedEmployee.name}</p>
                                    <p><strong>Email:</strong> {selectedEmployee.email}</p>
                                    <p><strong>Phone Number:</strong> {selectedEmployee.phonenumber}</p>

                                    <button onClick={handleDelete} className="delete-button">
                                        Delete Employee
                                    </button>
                                    <button onClick={handleClearSelection} className="clear-button">
                                        Clear Selection
                                    </button>
                                </div>
                            ) : (
                                <p className="user-placeholder">Select an Employee to see details</p>
                            )}
                        </div>


                        <div className="form-container">
                            <form onSubmit={handleSubmit} className="form">
                                <h2>{selectedEmployee ? "Update Employee" : "Create New Employee"}:</h2>
                                <input type="text" name="name" value={formData.name || ""} placeholder="Name"
                                       onChange={handleChange} required/>
                                <input type="email" name="email" value={formData.email || ""} placeholder="Email"
                                       onChange={handleChange} required/>
                                <input type="text" name="phonenumber" value={formData.phonenumber || ""}
                                       placeholder="Phone Number"
                                       onChange={handleChange} required/>
                                <button
                                    type="submit">{selectedEmployee ? "Update Employee" : "Create Employee"}</button>
                            </form>
                            {message && <p>{message}</p>}
                        </div>
                    </div>
                </div>
            </div>
        </main>
    );
}

export default NewEmployee;
