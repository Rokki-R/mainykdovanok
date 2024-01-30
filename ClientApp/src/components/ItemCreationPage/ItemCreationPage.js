import React, { useEffect, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import axios from 'axios';
import { Form, Button, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import './ItemCreationPage.css'

const ItemCreationPage = () => {
    const [name, setName] = useState('');
    const [images, setImages] = useState([]);
    const [description, setDescription] = useState('');
    const [location, setLocation] = useState('');
    const [category, setCategory] = useState('Pasirinkite kategoriją');
    const [categories, setCategories] = useState([]);
    const [itemType, setType] = useState('Pasirinkite, kaip norite atiduoti');
    const [itemTypes, setItemTypes] = useState([]);
    const [endDate, setEndDate] = useState('Pasirinkite datą');
    const navigate = useNavigate();

    const questionArray = [
        {
            type: "text",
            id: "1",
            value: ""
        }
    ];
    const [questions, setQuestions] = useState(questionArray);
    const addInput = () => {
        setQuestions(s => {
            return [
                ...s,
                {
                    type: "text",
                    value: ""
                }
            ];
        });
    };

    const handleChange = e => {
        e.preventDefault();

        const index = e.target.id;
        setQuestions(s => {
            const newArr = s.slice();
            newArr[index].value = e.target.value;

            return newArr;
        });
    };


    useEffect(() => {
        Promise.all([
            axios.get("api/item/getCategories"),
            axios.get("api/item/getItemTypes")
        ])
            .then(([categoriesResponse, itemTypesResponse]) => {
                setCategories(categoriesResponse.data);
                setItemTypes(itemTypesResponse.data);
            })
            .catch(error => {
                console.log(error);
                toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            });
    }, []);

    useEffect(() => {
        if (images.length < 1) return;
        if (images.length > 6) {
            toast.error("Negalima įkelti daugiau nei 6 nuotraukų!", {
                style: {
                    backgroundColor: 'red',
                    color: 'white',
                },
            });
            return;
        }
    }, [images]);

    const getAllCategories = () => {
        try {
            return categories.map((category) => {
                return <option value={category.id}>{category.name}</option>;
            });
        }
        catch (error) {
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            console.log(error);
        }
    }

    const getAllItemTypes = () => {
        try {
            return itemTypes.map((itemType) => {
                return <option value={itemType.id}>{itemType.name}</option>;
            });
        }
        catch (error) {
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            console.log(error);
        }
    }

    const getAllImages = () => {
        if (images.length > 0) {
            return images.map((image) => {
                const imageUrl = URL.createObjectURL(image);
                return <img src={imageUrl} style={{ maxWidth: '15%', height: 'auto', marginRight: '10px', border: '1px solid white' }}></img>;
            })
        }
    }

    function checkFields() {
        let containsEmptyQuestions = false;
        questions.forEach((question) => {
            if (question.value.trim() === "") {
                containsEmptyQuestions = true;
            }
        });
        if (name === '' || description === '' || location === '' || category === 'Pasirinkite kategoriją' || itemType === 'Pasirinkite, kaip norite atiduoti' || endDate === 'Pasirinkite datą' || containsEmptyQuestions) {
            toast.error('Reikia užpildyti visus laukus!');
            return false;
        }
        else {
            return true;
        }
    }

    const handleCreate = (event) => {
        event.preventDefault()
        if (checkFields()) {
            try {
                const formData = new FormData();
                formData.append('name', name);
                formData.append('description', description);
                formData.append('location', location);
                formData.append('category', category);
                formData.append('type', itemType);
                formData.append('endDate', endDate);
                for (let i = 0; i < questions.length; i++) {
                    formData.append('questions', questions[i].value);
                }
                for (let i = 0; i < images.length; i++) {
                    formData.append('images', images[i]);
                }
                axios.post("api/item/create", formData)
                    .then(response => {
                        if (response.status === 200) {
                            toast.success('Sėkmingai sukūrėtė skelbimą!');
                            navigate(`/skelbimas/${response.data}`)
                        }
                        else if (response.status === 401) {
                            toast.error('Turite būti prisijungęs!')
                            navigate('/prisijungimas');
                        }
                        else {
                            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
                        }
                    })
                    .catch(error => {
                        if (error.response.status === 401) {
                            toast.error('Turite būti prisijungęs!')
                            navigate('/prisijungimas');
                        }
                        else {
                            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
                        }
                    })
            }
            catch (error) {
                toast.error("Įvyko klaida, susisiekite su administratoriumi!");
            }
        }
    }

    const handleCancel = () => {
        navigate("/");
    }

    const today = new Date().toISOString().split('T')[0];

    return (
        <div className='page-container'>
            <div className='outerBoxWrapper'>
                <Card className='custom-card'>
                    <Toaster />
                    <Card.Header className='header d-flex justify-content-between align-items-center'>
                        <div className='text-center'>Skelbimo sukūrimas</div>
                    </Card.Header>
                    <Card.Body>
                        <Form>
                            <Form.Group className='mb-3'>
                                <Form.Control
                                    type='file'
                                    name='images'
                                    multiple accept='image/*'
                                    onChange={(e) => setImages([...e.target.files])} />
                            </Form.Group>
                            <Form.Group>
                                {getAllImages()}
                            </Form.Group>
                            <Form.Group className='text-center mt-3'>
                                <Form.Control
                                    className='input'
                                    type='text'
                                    name='name'
                                    id='name'
                                    value={name}
                                    onChange={(event) => setName(event.target.value)}
                                    placeholder='Pavadinimas'
                                />
                            </Form.Group>
                            <Form.Group className='text-center form-control-description mb-3'>
                                <Form.Control
                                    as='textarea'
                                    name='description'
                                    id='description'
                                    value={description}
                                    onChange={(e) => setDescription(e.target.value)}
                                    placeholder='Aprašymas' />
                            </Form.Group>
                            <Form.Group className='text-center'>
                                <Form.Control
                                    className='input'
                                    type='text'
                                    name='location'
                                    id='location'
                                    value={location}
                                    onChange={(event) => setLocation(event.target.value)}
                                    placeholder='Gyvenamoji vieta'
                                />
                            </Form.Group>
                            <Form.Group className='text-center mb-3'>
                                <Form.Select value={category} onChange={(e) => setCategory(e.target.value)}>
                                    <option>Pasirinkite kategoriją</option>
                                    {getAllCategories()}
                                </Form.Select>
                            </Form.Group>
                            <Form.Group className='text-center mb-3'>
                                <Form.Select value={itemType} onChange={(e) => setType(e.target.value)}>
                                    <option>Pasirinkite, kaip norite atiduoti</option>
                                    {getAllItemTypes()}
                                </Form.Select>
                            </Form.Group>
                            {itemType === '2' && (
                                <>
                                    {questions.map((item, i) => {
                                        return (
                                            <Form.Group className="d-flex align-items-center mb-2">
                                                <Form.Control
                                                    onChange={handleChange}
                                                    value={item.value}
                                                    id={i.toString()}
                                                    type={item.type}
                                                    placeholder='Įrašykite klausimą'
                                                    className='questionInput'
                                                />
                                                <div className='addQuestion mt-2 ml-5'>
                                                    {questions.length - 1 === i && <Button className='btn btn-primary' onClick={addInput}>+</Button>}
                                                </div>
                                            </Form.Group>
                                        );
                                    })}
                                </>
                            )}
                            <Form.Group>
                                <Form.Label>Pasirinkite skelbimo pabaigos datą:</Form.Label>
                                <Form.Control
                                    type='datetime-local'
                                    value={endDate || ''}
                                    onChange={(e) => {
                                        const selectedDate = new Date(e.target.value);
                                        const currentDate = new Date();
                                        if (selectedDate < currentDate) {
                                            return;
                                        }
                                        setEndDate(e.target.value);
                                    }}
                                    min={today}
                                    placeholder='Pasirinkite skelbimo pabaigos datą'
                                />
                            </Form.Group>
                            <div className='d-flex justify-content-between'>
                                <Button onClick={(event) => handleCreate(event)} type='submit'>Sukurti</Button>
                                <Button variant='secondary' onClick={() => handleCancel()} type='button'>Atšaukti</Button>
                            </div>
                        </Form>
                    </Card.Body>
                </Card>
            </div>
        </div>
    )
}
export default ItemCreationPage