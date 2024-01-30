import React, { useState } from 'react';
import { Card, CardHeader, CardBody, Form, FormGroup, Label, Input, Button } from 'reactstrap';
import { useNavigate } from 'react-router';
import toast, { Toaster } from 'react-hot-toast';
import { Link } from 'react-router-dom';
import './RegistrationPage.css'

const RegistrationPage = () => {
  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [email, setEmail] = useState('');
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
    }
}

function checkFields() {
    if (password === confirmPassword) {
        if (name === '' || surname === '' || email === '') {
            setMessage('Reikia užpildyti visus laukus!');
            return false;
        }
        else if (password.length >= 8 && /^(?=.*\d)(?=.*[!@#$%^&*+\-])(?=.*[a-z])(?=.*[A-Z]).{8,}$/.test(password))
        {
            if  (!/\S+@\S+\.\S+/.test(email))
            {
                setMatchMessage("Neteisingai įvestas el. paštas");
                return false;
            }
        setMatchMessage("");
        setMessage("");
        return true;
        }
    }
    else {
        setMatchMessage("Slaptažodiai turi sutapti!");
        return false;
    }
}

const handleSubmit = (event) => {
  event.preventDefault()
  if (checkFields()) {
      const requestOptions = {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
              name: name,
              surname: surname,
              password: password,
              email: email,
          }),
      };
      fetch("api/user/register", requestOptions)
          .then(response => {
              if (response.status === 200) {
                  toast.success('Sėkmingai prisiregistravote. Elektroninio pašto patvirtinimas išsiųstas.');
                  navigate("/prisijungimas");
              }
              else if (response.status === 401) {
                  toast.error("Nepavyko išsiųsti patvirtinimo laiško! Susisiekite su administratoriumi.", {
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
}

  return (
    <div className='page-container'>
    <div className='outerBoxWrapper'>
      <Card className='custom-card'>
        <CardHeader className='header d-flex justify-content-between align-items-center'>Registration</CardHeader>
        <CardBody>
          <Form onSubmit={handleSubmit}>
            <FormGroup>
              <Label for="name">Name</Label>
              <Input
                type="text"
                className='input'
                name="name"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter your name"
              />
            </FormGroup>
            <FormGroup>
              <Label for="surname">Surname</Label>
              <Input
                type="text"
                className='input'
                name="surname"
                id="surname"
                value={surname}
                onChange={(e) => setSurname(e.target.value)}
                placeholder="Enter your surname"
              />
            </FormGroup>
            <FormGroup>
              <Label for="email">Email</Label>
              <Input
                type="email"
                className='input'
                name="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Enter your email"
              />
            </FormGroup>
            <FormGroup>
              <Label for="password">Password</Label>
              <Input
                type="password"
                className='input'
                name="password"
                id="password"
                value={password}
                onChange={onChange}
                placeholder="Enter your password"
              />
            </FormGroup>
            <FormGroup>
              <Label for="confirmPassword">Repeat Password</Label>
              <Input
                type="password"
                className='input'
                name="confirmPassword"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Repeat your password"
              />
            </FormGroup>
            <Label className='warningText'>{message}</Label>
            <Label className='warningText'>{matchMessage}</Label>
            <div className='text-center'>
                <Button className='btn btn-primary' onClick={(event) => handleSubmit(event)} type='submit'>
                Registruotis
                </Button>
            </div>
            <div className="mt-3 text-center">
                Already have an account? <Link to="/login">Login</Link>
            </div>
          </Form>
        </CardBody>
      </Card>
    </div>
    </div>
  );
};

export default RegistrationPage;
