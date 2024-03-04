import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import './PasswordChangePage.css'
import toast, { Toaster } from 'react-hot-toast';
import { Form, Button, Alert, Card } from 'react-bootstrap';

const PasswordChangePage = () => {

    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [message, setMessage] = useState('');
    const [matchMessage, setMatchMessage] = useState('');
    const navigate = useNavigate();

    const onChange = (e) => {
        let password = e.target.value;
        setPassword(password);
        if (password.length >= 8 && /^(?=.*\d)(?=.*[!@#$%^&*+\-])(?=.*[a-z])(?=.*[A-Z]).{8,}$/.test(password)) {
            setMessage("");
        }
        else {
            setMessage("Slaptažodis turi turėti mažąsias, didžiąsias raides, skaičius, spec. simbolius ir būti bent 8 simbolių ilgio!");

            if (password.length > 0 && confirmPassword.length > 0) {
                setMatchMessage("");   
            }
        }
    }

    function checkFields() {
        if (password === confirmPassword && password.length >= 8 && confirmPassword.length >= 8 && /^(?=.*\d)(?=.*[!@#$%^&*+\-])(?=.*[a-z])(?=.*[A-Z]).{8,}$/.test(password)) {
            setMatchMessage("");
            setMessage("");
            return true;
        }
        else if (password.length === 0 || confirmPassword.length === 0) {
            setMatchMessage("Slaptažodžių laukai turi būti užpildyti!");
            return false;
        }
        else {
            setMatchMessage("Slaptažodžiai turi sutapti!");
            return false;
        }
    }

    const handleSubmit = (event) => {
        event.preventDefault()
        if (checkFields()) {
            const urlParams = new URLSearchParams(window.location.search);
            const email = urlParams.get('email');
            const token = urlParams.get('token');
            const requestOptions = {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    password: password,
                    email: email,
                    token: token
                }),
            };
            fetch("api/user/changePassword", requestOptions)
                .then(response => {
                    if (response.status === 200) {
                        toast.success('Slaptažodis sėkmingai pakeistas!');
                        navigate("/prisijungimas");
                    }
                    else if (response.status === 401) {
                        toast.error("Neteisingi nuorodos duomenys. Pakartokite slaptažodžio pakeitimo užklausą.");
                    }
                    else if (response.status === 300) {
                        toast.error("Pasibaigė laikotarpios pakeisti slaptažodžiui. Pakartokite slaptažodžio pakeitimo užklausą.")
                    }
                    else {
                        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
                    }
                }
                );
        }
    }

    return (
        <div className='outerBoxWrapper'>
            <Card>
                <Toaster />
                <Card.Header className='header d-flex justify-content-between align-items-center'>
                    <div>Slaptažodžio keitimas</div>
                </Card.Header>
                <Card.Body>
                    <Form>
                        <Form.Group controlId="password">
                            <Form.Label className="label">Naujas slaptažodis</Form.Label>
                            <Form.Control type="password" name='password' id='password' value={password} onChange={onChange} placeholder='Slaptažodis' />
                        </Form.Group>
                        <Form.Group controlId="confirmPassword">
                            <Form.Label className="label">Pakartoti naują slaptažodį</Form.Label>
                            <Form.Control type="password" name='confirmPassword' id='confirmPassword' value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} placeholder='Pakartokite slaptažodį' />
                        </Form.Group>
                        {message && <Alert variant="danger">{message}</Alert>}
                        {matchMessage && <Alert variant="danger">{matchMessage}</Alert>}
                        <Button className='change' type="submit" onClick={(event) => handleSubmit(event)} >
                            Patvirtinti
                        </Button>
                        <div className="returnToLogin">
                            <a href="/prisijungimas" className="returnToLoginButton">Grįžti į prisijungimą</a>
                        </div>
                    </Form>
                </Card.Body>
            </Card>
        </div>
    )
}
export default PasswordChangePage