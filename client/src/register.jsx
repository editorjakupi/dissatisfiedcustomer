import './register.css'

export default function RegisterMain()
{
  return <main>
    <div id="input-div"> 
    <p>Full name:</p>
      <label>
        <input type='text' name='name-input' placeholder='Full name' />
      </label>
      <p>Email-address:</p>
      <label>
        <input type='text' name='email-address-input' placeholder='example@mail.com' />
      </label>
      <p>Phone-number:</p>
      <label>
        <input type='text' name='phone-number-input' placeholder='076000000' />
      </label>
      <p>Password:</p>
      <label>
        <input type='Password' name='password-input' placeholder='Password' />
      </label>
      <p>Confirm password:</p>
      <label>
        <input type='Password' name='confirm-password-input' placeholder='Confirm Password' />
      </label>
    </div>
    <div id='register-button-div'>
      <form>
        <button>Register</button>
      </form>
    </div>
  </main>
}