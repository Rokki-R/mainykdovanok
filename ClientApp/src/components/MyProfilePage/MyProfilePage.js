import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Card, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import toast, { Toaster } from 'react-hot-toast';
import axios from 'axios';
import './MyProfilePage.css'

const MyProfilePage = () => {
    const [message, setMessage] = useState('');
    const [user, setUser] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        async function fetchUser() {
            try {
                await axios.get("api/user/getMyProfileDetails").then(response => {
                    const data = response.data;
                    console.log(data)
                    setUser({ ...data });
                });    
            }
            catch (error) {
                if (error.response.status === 401) {
                    toast.error('Jūs turite būti prisijungęs');
                    navigate('/prisijungimas');
                }
            };
        }
        fetchUser();
    }, []);

    function checkFields(formData) {
        const name = formData.get('name');
        const surname = formData.get('surname');
        const phoneNumber = formData.get('phone_number')

        if ((name === '' || surname === '' || phoneNumber === '') ) {
            toast.error('Naudotojo duomenys negali būti tušti!', {
                style: {
                    backgroundColor: 'red',
                    color: 'white',
                },
            });
            return false;
        }

        else if (!/^(?:\+[0-9]{1,3} ?)?[0-9]{8,14}$/.test(phoneNumber))
            {
                toast.error('Įvestas neteisingas telefono numeris', {
                    style: {
                        backgroundColor: 'red',
                        color: 'white',
                    },
                });
                return false;
            }

        return true;
    }

    const handleSubmit = event => {
        event.preventDefault();
        
        if (message !== '' || (message !== '')) {
            return;
        }

        const formData = new FormData();
        formData.append('name', user.name);
        formData.append('surname', user.surname);
        formData.append('phone_number', user.phone_number)

        if (checkFields(formData)) {
            axios.post("api/user/updateProfileDetails", formData)
            .then(response => {
                if (response.status === 200) {
                    toast.success('Duomenys sėkmingai išsaugoti!', {
                        style: {
                            backgroundColor: 'rgb(14, 160, 14)',
                            color: 'white',
                        },
                    });
                }
                else {
                    toast.error("Įvyko klaida, susisiekite su administratoriumi!");
                }
            })
            .catch(error => {
                if (error.response.data) {
                    toast.error(error.response.data);
                }
                else {
                    toast.error("Įvyko klaida, susisiekite su administratoriumi!");
                }
            });
        }
    }

    return user ? (
        <Container className="profile">
        <Toaster></Toaster>
            <Row>
                <Col md={16}>
                    <Card>
                        <Card.Body>
                            <div className="info">
                                <Form>
                                    <Form.Group>
                                        <Form.Label><strong>Vardas:</strong></Form.Label>
                                        <Form.Control type="text" id="name" value={user.name} onChange={(e) => setUser({ ...user, name: e.target.value })} />
                                    </Form.Group>
                                    <Form.Group>
                                        <Form.Label><strong>Pavardė:</strong></Form.Label>
                                        <Form.Control type="text" id="surname" value={user.surname} onChange={(e) => setUser({ ...user, surname: e.target.value })} />
                                    </Form.Group>
                                    <Form.Group>
    <Form.Label><strong>El. paštas:</strong></Form.Label>
    <Form.Control type="email" id="email" value={user.email} readOnly />
</Form.Group>
<Form.Group>
    <Form.Label><strong>Telefono numeris:</strong></Form.Label>
    <Form.Control type="text" id="phone_number" value={user.phone_number} onChange={(e) => setUser({ ...user, phone_number: e.target.value })} />
</Form.Group>

                                    <Form.Group className='givenAwayLabel'>
                                        <Form.Label><strong>Atiduotų elektronikos prietaisų kiekis:</strong> {user.devicesGifted}</Form.Label>
                                    </Form.Group>
                                    <Form.Group>
                                        <Form.Label><strong>Laimėtų elektronikos prietaisų kiekis:</strong> {user.devicesWon}</Form.Label>
                                    </Form.Group>
                                    <div className="d-flex flex-column">
                                        <Form.Text className="text-danger">{message}</Form.Text>
                                        <Button className="save-button mt-3" onClick={(e) => handleSubmit(e)} type='submit'>Išsaugoti</Button>
                                    </div>
                                </Form>
                            </div>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );
};
export default MyProfilePage;