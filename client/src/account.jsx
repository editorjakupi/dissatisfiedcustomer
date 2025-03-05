import './account.css';


const AccountInformation = ({ user, setUser }) => {

  const handleUpdate = async (event) => {
    event.preventDefault(); // Ensure form submission handling

    const formData = new FormData(event.target);

    const updatedUser = {
      id: user.userId,
      name: formData.get("name-input") || user.name,
      email: formData.get("email-address-input") || user.email,
      phonenumber: formData.get("phone-number-input") || user.phonenumber,
      password: formData.get("password-input") || "", // Empty means no change
      role_id: user.role_id,
      companyId: user.companyId
    };

    try {
      const response = await fetch("/api/users", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updatedUser),
      });

      if (!response.ok) throw new Error("Failed to update user");
      alert('Password Updated: ' + user.password);
    } catch (error) {
      console.error("Error updating user:", error);
    }
  };

  return (
      <main>
        <form onSubmit={handleUpdate}>
          <div id="input-div">
            <div className="input-type-div">
              <label>
                <p>Full name: (Optional)</p>
                <input type='text' name='name-input' placeholder={user?.name}/>
              </label>
            </div>

            <div className="input-type-div">
              <label>
                <p>Email address: (Optional)</p>
                <input type='text' name='email-address-input' placeholder={user?.email}/>
              </label>
            </div>

            <div className="input-type-div">
              <label>
                <p>Phone number: (Optional)</p>
                <input type='text' name='phone-number-input' placeholder={user?.phonenumber}/>
              </label>
            </div>

            <div className="input-type-div">
              <label>
                <p>New Password: (Optional)</p>
                <input type='password' name='password-input' placeholder='New Password'/>
              </label>
            </div>
            <div id="update-button-div">
              <button type="submit">Update</button>
            </div>
          </div>
        </form>
      </main>
  );

};

export default AccountInformation;