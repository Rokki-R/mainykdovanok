import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import toast, { Toaster } from 'react-hot-toast';
import { Spinner, Form, Button, Card, InputGroup } from 'react-bootstrap';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import './ItemUpdatePage.css';

function ItemUpdatePage() {
  const { itemId } = useParams();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [viewerId, setViewerId] = useState(null);
  const [category, setCategory] = useState('Pasirinkite kategoriją');
  const [categories, setCategories] = useState([]);
  const [itemType, setType] = useState('Pasirinkite, kaip norite atiduoti');
  const [itemTypes, setItemTypes] = useState([]);
  const [images, setImages] = useState([]);
  const [imagesToDelete, setImagesToDelete] = useState([]);
  const [item, setItem] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [showMessage, setShowMessage] = useState(false);
  const [isLoggedInAsAdmin, setIsLoggedInAsAdmin] = useState(false);

  const navigate = useNavigate();


  useEffect(() => {
    async function fetchItem() {
      const response = await axios.get(`/api/item/getItem/${itemId}`);
      setItem(response.data);
      setName(response.data.name);
      setDescription(response.data.description);
      setCategory(response.data.fk_Category);
      console.log(response.data.type);
  
      if (response.data.type === 'Klausimynas') {
        try {
          const questionsResponse = await axios.get(`/api/item/getQuestions/${itemId}`);
          setQuestions(questionsResponse.data.questionnaires);
          console.log(questionsResponse.data);
        } catch (error) {
          console.error('Failed to fetch questions:', error);
        }
      }
    }
    fetchItem();
  }, [itemId]);
  
  
  useEffect(() => {
    async function fetchData() {
        try {
            const [categoriesResponse, itemTypesResponse] = await Promise.all([
                axios.get("api/item/getCategories"),
                axios.get("api/item/getItemTypes")
            ]);
            setCategories(categoriesResponse.data);
            setItemTypes(itemTypesResponse.data);
        } catch (error) {
            console.log(error);
            toast.error("Įvyko klaida, susisiekite su administratoriumi!");
        }
    }
    fetchData();
}, []);

  
  
  useEffect(() => {
    const fetchViewerId = async () => {
      try {
        const response = await axios.get('api/user/getCurrentUserId');
        setViewerId(response.data);
      } catch (error) {
        if (error.response.status === 401) {
          navigate('/prisijungimas');
          toast.error('Turite būti prisijungęs!');
        }
        else {
          toast.error('Įvyko klaida, susisiekite su administratoriumi!');
        }
      }
    };
    fetchViewerId();
  }, []);




  if (!isLoggedInAsAdmin) {
    if (item && viewerId && item.userId !== viewerId) {
      navigate('/');
      toast.error('Jūs nesate šio skelbimo savininkas');
    }
  }


  const handleSubmit = async (event) => {

    try {
      event.preventDefault();
      const data = new FormData();
      data.append('name', name || item.name);
      data.append('description', description || item.description);
      data.append('fk_Category', category || item.fk_Category);
      data.append('type', itemType || item.type);
      if (item.images.length === 0 && images.length === 0) {
        toast.error('Negalite palikti skelbimo be nuotraukos');
        await new Promise(resolve => setTimeout(resolve, 1000));
        window.location.reload();
        return;
      }
      if (name === '' || description === '' || category === 'Pasirinkite kategoriją'|| itemType === 'Pasirinkite, kaip norite atiduoti') {
        toast.error('Užpildykite visus laukus!');
        return;
      }
      if (itemType === '4') {
        let hasEmptyQuestion = false;
        questions.forEach((question) => {
            if (question.question.trim() === "") {
                hasEmptyQuestion = true;
                return;
            }
        });
        if (hasEmptyQuestion || questions.length === 0) 
        {
            toast.error(`Negalite palikti tuščių klausimų!`);
            return;
        }
    }
      
      if (images.length > 6 || images.length + item.images.length > 6) {
        toast.error('Daugiausiai galite įkelti 6 nuotraukas');
        await new Promise(resolve => setTimeout(resolve, 1000));
        window.location.reload();
        return;
      }

      for (let i = 0; i < images.length; i++) {
        data.append('images', images[i]);
      }

      for (let i = 0; i < imagesToDelete.length; i++) {
        data.append('imagesToDelete', imagesToDelete[i]);
      }

      for (let i = 0; i < questions.length; i++) {
        data.append('questions', questions[i].question);
        }

      await axios.put(`/api/item/update/${itemId}`, data);
      toast.success('Skelbimas sėkmingai atnaujintas!');
      setName('');
      setDescription('');
      setCategory('');
      setImages([]);
      setImagesToDelete([]);
      navigate(`/skelbimas/${itemId}`);
    } catch (error) {
        console.log(error);
        if (error.response) {
          toast.error(error.response.data);
        } else {
          toast.error('Ivyko klaida, susisiekite su administratoriumi!');
        }
      }
  };

  const removeSelectedImage = (indexToRemove) => {
    setImages((prevImages) => prevImages.filter((_, index) => index !== indexToRemove));
  };

  const getAllImages = () => {
    if (images.length > 0) {
      return images.map((image, index) => {
        const imageUrl = URL.createObjectURL(image);
        return (
          <div key={index} style={{ position: 'relative', display: 'inline-block' }}>
            <img src={imageUrl} style={{ width: '128px', height: 'auto', marginRight: '15px', border: '1px solid white' }} alt={`Selected Image ${index}`} />
            <button
              type="button"
              className="btn btn-sm btn-danger remove-image-btn"
              style={{ position: 'absolute', top: '5px', right: '5px' }}
              onClick={() => removeSelectedImage(index)}
            >
              X
            </button>
          </div>
        );
      });
    }
  };
  


  function getExistingImages() {
    return (
      <div className="d-flex flex-wrap">
        {item.images.map((image, index) => (
          <Form.Group key={index} className="d-inline-block mr-3" style={{ width: '128px', marginRight: '10px' }}>
            <img
              className="image-preview"
              src={`data:image/png;base64,${image.data}`}
              alt={`Image ${index + 1}`}
              height="320"
              style={{ border: '1px solid white' }}
            />
            <div>
              <Button
                variant="secondary"
                onClick={() => handleDeleteImage(image.id)}
              >
                Pašalinti
              </Button>
            </div>
          </Form.Group>
        ))}
      </div>
    );
  }
  

  function handleDeleteImage(id) {
    setImagesToDelete((prevImagesToDelete) => [...prevImagesToDelete, id]);
    setShowMessage(true);
    setItem((prevItem) => ({
      ...prevItem,
      images: prevItem.images.filter((image) => image.id !== id),
    }));
  }

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
  
  const handleCancel = () => {
    navigate(`/skelbimas/${itemId}`);
  }

  if (!item || !categories) {
    return <div><Spinner>Loading...</Spinner></div>;
  }

  const handleQuestionChange = (e, index) => {
    const { value } = e.target;
    setQuestions((prevQuestions) => {
      const updatedQuestions = [...prevQuestions];
      updatedQuestions[index].question = value;
      return updatedQuestions;
    });
  };
  
  
  const addQuestion = () => {
    setQuestions((prevQuestions) => [
      ...prevQuestions,
      { question: '' }
    ]);
  };
  
  const removeQuestion = (indexToRemove) => {
    setQuestions((prevQuestions) => prevQuestions.filter((_, index) => index !== indexToRemove));
  };

  

  return (
    <div className='page-container'>
      <div className='outerBoxWrapper'>
        <Card className='custom-card'>
          <Toaster />
          <Card.Header className='header d-flex justify-content-between align-items-center'>
            <div className='text-center'>Skelbimo atnaujinimas</div>
          </Card.Header>
          <Card.Body>
            <Form>
              <Form.Group>
                {getExistingImages()}
              </Form.Group>
              <Form.Group className='mb-3 mt-3'>
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

<Form.Group>
{item && item.type === 'Klausimynas' && (itemType === 'Pasirinkite, kaip norite atiduoti' || itemType === '4') && (
    <>
      <h4>Klausimai:</h4>
      {questions.map((question, index) => (
        <div key={index} className="mb-3">
          <InputGroup>
            <Form.Control
              type="text"
              value={question.question}
              onChange={(e) => handleQuestionChange(e, index)}
              placeholder={`Klausimas ${index + 1}`}
            />
            <Button variant="danger" onClick={() => removeQuestion(index)}>-</Button>
          </InputGroup>
        </div>
      ))}
      <Button variant="primary" onClick={addQuestion}>Pridėti klausimą</Button>
    </>
  )}
  {item && item.type !== 'Klausimynas' && itemType === '4' && (
    <>
      <h4>Klausimai:</h4>
      {questions.map((question, index) => (
        <div key={index} className="mb-3">
          <InputGroup>
            <Form.Control
              type="text"
              value={question.question}
              onChange={(e) => handleQuestionChange(e, index)}
              placeholder={`Klausimas ${index + 1}`}
            />
            <Button variant="danger" onClick={() => removeQuestion(index)}>-</Button>
          </InputGroup>
        </div>
      ))}
      <Button variant="primary" onClick={addQuestion}>Pridėti klausimą</Button>
      </>
  )}
</Form.Group>



              <div className='d-flex justify-content-between'>
                <Button onClick={(event) => handleSubmit(event)} type='submit'>Atnaujinti</Button>
                <Button onClick={(event) => handleCancel(event)} type='submit'>Atšaukti</Button>
              </div>
            </Form>
          </Card.Body>
        </Card>
      </div>
    </div>
  );
}

export default ItemUpdatePage;