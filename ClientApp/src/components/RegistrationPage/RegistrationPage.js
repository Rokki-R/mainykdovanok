import React, { useState } from 'react';
import { Card, CardHeader, CardBody, Form, FormGroup, Label, Input, Button, } from 'reactstrap';
import { Container } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import toast, { Toaster } from 'react-hot-toast';
import { Link } from 'react-router-dom';
import './RegistrationPage.css'

const RegistrationPage = () => {
  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
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
        if (name === '' || surname === '' || email === '' || phoneNumber === '') {
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
            else if (!/^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$/.test(phoneNumber)) {
              setMatchMessage("Neteisingai įvestas telefono numeris");
              return false;
            }
        setMatchMessage("");
        setMessage("");
        return true;
        }
    }
    else {
        setMatchMessage("Slaptažodžiai turi sutapti!");
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
              phoneNumber: phoneNumber,
              email: email,
              confirmPassword: confirmPassword
          }),
      };
      console.log(phoneNumber)
      fetch("api/login/register", requestOptions)
          .then(response => {
              if (response.status === 200) {
                  toast.success('Sėkmingai prisiregistravote.');
                  navigate("/prisijungimas");
                } else if (response.status === 400) {
                  response.json().then(data => {
                      toast.error(data.message);
                  });
              } else {
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
    <Container className="my-5 outerRegistrationBoxWrapper">
      <Card className='custom-card'>
      <CardHeader className='header d-flex justify-content-between align-items-center' style={{ color: 'black' }}>Registracija</CardHeader>
        <CardBody>
          <Form onSubmit={handleSubmit}>
            <FormGroup>
              <Input
                type="text"
                className='input'
                name="name"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Įveskite savo vardą"
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="text"
                className='input'
                name="surname"
                id="surname"
                value={surname}
                onChange={(e) => setSurname(e.target.value)}
                placeholder="Įveskite savo pavardę"
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="email"
                className='input'
                name="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Įveskite savo elektroninį paštą"
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="text"
                className='input'
                name="phoneNumber"
                id="phoneNumber"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                placeholder="Įveskite savo telefono numerį"
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="password"
                className='input'
                name="password"
                id="password"
                value={password}
                onChange={onChange}
                placeholder="Įveskite slaptažodį"
              />
            </FormGroup>
            <FormGroup>
              <Input
                type="password"
                className='input'
                name="confirmPassword"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Pakartokite slaptažodį"
              />
            </FormGroup>
            <Label className='warningText'>{message}</Label>
            <Label className='warningText'>{matchMessage}</Label>
            <div className='text-center'>
            <Button className='btn btn-primary' onClick={(event) => handleSubmit(event)} type='submit'>
              Registruotis
            </Button>
          </div>
            <hr></hr>
            <div className="mt-3 text-center">
                Jau turite paskyrą? <Link to="/login">Prisijungti</Link>
            </div>
          </Form>
        </CardBody>
      </Card>
    </Container>
  );
};

export default RegistrationPage;
