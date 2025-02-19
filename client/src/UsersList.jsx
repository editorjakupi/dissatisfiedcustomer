import {useEffect, useState} from "react";

const UsersList = () => {
    const [users, setUsers] = useState();
    const [selectedUser, setSelectedUser] = useState(null);

    useEffect(() => {
        fetch("/api/users")
            .then((res) => res.json())
            .then((data) => setUsers(data))
            .catch((err) => console.error("Error Fetching Users:", err))
    }, []);
    
    return (
        <div className="flex p-4">
            {/* Users List */}
            <ul>
                {users.map((newUser) => (
                    <li key={newUser.id} className="" onClick={() => setSelectedUser(newUser)}>
                        {newUser.name}
                    </li>
                ))}
            </ul>
            
            <div>
                {selectedUser ? (
                    <div>
                        <h2>{selectedUser.name}</h2>
                        <p>Email: {selectedUser.email}</p>
                        <p>Phone: {selectedUser.phonenumber}</p>
                        <p>Password: {selectedUser.password}</p>
                    </div>
                ) : (
                    <p>Select a User to view details.</p>
                )}
            </div>
        </div>
    )
}

export default UsersList;