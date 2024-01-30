import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import { Carousel, Col, Container, Row, Form, Button, Card, Spinner, Collapse } from 'react-bootstrap';
import './LoginPage.css'
import toast, { Toaster } from 'react-hot-toast';
import axios from 'axios';

export const LoginPage = () => {

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchUserLogin = async () => {
            try {
                const response = await axios.get('api/user/isloggedin/0');
                if (response.status == 200)
                {
                  toast.error("Jūs jau esate prisijungęs!")
                  navigate('/');
                }
            } catch (error) {
              if (error.response.status === 401) {
                return true;
              }
              else
              {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
              }
            }
        };
        fetchUserLogin();
      }, []);

    const handleSubmit = (event) => {
        event.preventDefault();

        if (password.length < 8 || /^(?=.*\d)(?=.*[!@#$%^&*+\-])(?=.*[a-z])(?=.*[A-Z]).{8,}$/.test(password) === false) {
            toast.error("Prisijungimo duomenys neteisingi!");
            return;
        }

        const requestOptions = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                password: password,
                email: email
            }),
        };
        fetch("api/user/login", requestOptions)
            .then(response => {
                if (response.status === 200) {
                    // Hack to make the NavMenu update the user avatar.
                    window.location.reload();
                    window.location.href = "/";
                }
                else if (response.status === 404) {
                    toast.error("Prisijungimo duomenys neteisingi!");
                }
                else if (response.status === 401) {
                    toast.error("El. pašto adresas nepatvirtintas. Patikrinkite savo elektroninį paštą!");
                }
                else {
                    toast.error("Įvyko klaida, susisiekite su administratoriumi!");
                }
            })
    }
    

    return (
        <div className='page-container'>
        <div className='outerBoxWrapper'>
          <Card className='custom-card'>
          <Toaster />
          <Card.Header className='header d-flex justify-content-between align-items-center'>
            <div className='text-center'>Prisijungimas</div>
          </Card.Header>
              <Card.Body>
              <Form>
              <Form.Group className='text-center'>
                <Form.Control
                  className='input'
                  type='email'
                  name='email'
                  id='email'
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  placeholder='Įveskite el. paštą'
                />
              </Form.Group>
              <Form.Group className='text-center'>
                <Form.Control
                  className='input'
                  type='password'
                  name='password'
                  id='password'
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  placeholder='Įveskite slaptažodį'
                />
              </Form.Group>
              <div className='text-center'>
                <Button className='btn btn-primary' onClick={(event) => handleSubmit(event)} type='submit'>
                  Prisijungti
                </Button>
              </div>
              <div className='forgotPassword text-center'>
                <a href='/pamirsau-slaptazodi' className='forgotPasswordButton'>
                  Pamiršau slaptažodį
                </a>
              </div>
              <hr />
              <div className='register text-center'>
                <p className='noAccount'>Neturite paskyros?</p>
                <Button className='btn btn-primary'>
                  <a className='redirect' href='/registracija'>
                    Registruotis
                  </a>
                </Button>
              </div>
            </Form>
          </Card.Body>
        </Card>
      </div>
    </div>
    )
}