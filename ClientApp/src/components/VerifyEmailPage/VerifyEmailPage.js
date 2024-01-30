import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import toast, { Toaster } from 'react-hot-toast';

const VerifyEmailPage = () => {

    const navigate = useNavigate();

    useEffect(() => {
        handleSubmit();
    }, []);


    const handleSubmit = () => {
            const urlParams = new URLSearchParams(window.location.search);
            const email = urlParams.get('email');
            const token = urlParams.get('token');
            const requestOptions = {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    email: email,
                    token: token
                }),
            };
            fetch("https://localhost:7185/api/user/verifyEmail", requestOptions)
                .then(response => {
                    if (response.status === 200) {
                        toast.success('El. pašto adresas sėkmingai patvirtintas!');
                        navigate("/prisijungimas");
                    }
                    else if (response.status === 404) {
                        toast.error("Neteisingi nuorodos duomenys. Bandykite iš naujo arba susisiekite su administratoriumi.", {
                            style: {
                                backgroundColor: 'red',
                                color: 'white',
                            },
                        });
                    }
                    else {
                        toast.error("Įvyko klaida, susisiekite su administratoriumi!", {
                            style: {
                                backgroundColor: 'red',
                                color: 'white',
                            },
                        });
                    }
                })
        
    }

    return (<Toaster />)
}
export default VerifyEmailPage