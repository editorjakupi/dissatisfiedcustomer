import { useState } from "react";
import { useNavigate, Link } from "react-router";

const PasswordForget = () => {
    const [loading, setLoading] = useState(false);
    
    return (
            <div>
                <div className="w-[500px] p-5 rounded-sm shadow-lg bg-white bg-opacity-70">
                    <h1 className="text-xl lg:text-2xl font-bold">Forgot password ?</h1>
                    <p className="flex text-sm lg:text-base">
                        Don&#39;t worry it happens all the time. Write your email
                        below and we will send you a recovery email.
                    </p>
                    <p className="text-xs mt-2">
                        <span>(OBS: </span>Check your spam box)
                    </p>
                        <div className="mb-3 mt-4">
                            <label htmlFor="email">Email</label>
                            <input
                                type="email"
                                className="w-full mt-1"
                                placeholder="exemplo@email.com"
                                id="email"
                            />
                        </div>
                        <div className="mt-5">
                            <button
                                className="w-full bg-gray-500 hover:bg-gray-700 p-2 rounded-sm text-white"
                                disabled={loading}
                            >
                                {loading ? 'Processing' : 'Send'}
                            </button>
                        </div>
                </div>
            </div>
    );
};

export default PasswordForget;