import './account.css'

const AccountInformation =({user}) =>
{
  
  
  
  return <main>
    <div id="input-div"> 
      <div className='input-type-div'>
        <p>Full name:</p>
        <p>(Optional)</p>
      </div>
      <label>
        <input type='text' name='name-input' placeholder={user?.name} />
      </label>
      <div className='input-type-div'>
        <p>Email-address:</p>
        <p>(Optional)</p>
      </div>
      <label>
        <input type='text' name='email-address-input' placeholder={user?.email} />
      </label>
      <div className='input-type-div'>
        <p>Phone-number:</p>
        <p>(Optional)</p>
      </div>
      <label>
        <input type='text' name='phone-number-input' placeholder={user?.phoneNumber} />
        </label>
      <div className='input-type-div'>
        <p>Old Password:</p>
        <p>(Optional)</p>
      </div>
        <label>
          <input type='Password' name='old-password-input' placeholder='Password' />
        </label>
      <div className='input-type-div'>
        <p>Old Password:</p>
        <p>(Optional)</p>
      </div>
        <label>
          <input type='Password' name='confirm-old-password-input' placeholder='Confirm Password' />
        </label>
      <div className='input-type-div'>
        <p>Password:</p>
        <p>(Optional)</p>
      </div>
        <label>
          <input type='Password' name='password-input' placeholder='Password' />
        </label>
      <div className='input-type-div'>
        <p>Confirm Password:</p>
        <p>(Optional)</p>
      </div>
        <label>
          <input type='Password' name='confirm-password-input' placeholder='Confirm Password' />
        </label>
      </div>
    <div id='update-button-div'>
      <form>
        <button>Update</button>
      </form>
    </div>
  </main>
}

export default AccountInformation;