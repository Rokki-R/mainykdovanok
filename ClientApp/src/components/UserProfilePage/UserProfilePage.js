import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Container, Card, Spinner } from 'react-bootstrap';
import toast from 'react-hot-toast';
import axios from 'axios';

export const UserProfilePage = () => {
    const { userId } = useParams();
    const [user, setUser] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const userDetailsResponse = await axios.get(`api/user/getUserDetails/${userId}`);
                setUser(userDetailsResponse.data);
            } catch (error) {
                toast.error('Error fetching user profile information.');
            } finally {
                setIsLoading(false);
            }
        };

        fetchData();
    }, [userId]);
    return (
        <Container className='profile d-flex justify-content-center align-items-center'>
            {isLoading ? (
                <Spinner animation="border" role="status">
                    <span className="sr-only">Loading...</span>
                </Spinner>
            ) : (
                <Card className="my-4" style={{ width: '400px' }}>
                    <Card.Body className="text-center">
                        <Card.Title className="mb-4">
                            {user.name} {user.surname}
                        </Card.Title>
                        <Card.Text className="text-center mb-0">
                            <strong>El. paštas:</strong> {user.email}
                        </Card.Text>
                        <Card.Text className="text-center mb-0">
                            <strong>Padovanotų elektronikos prietaisų kiekis:</strong> {user.itemsGifted}
                        </Card.Text>
                        <Card.Text className="text-center mb-0">
                            <strong>Laimėtų elektronikos prietaisų kiekis:</strong> {user.itemsWon}
                        </Card.Text>
                    </Card.Body>
                </Card>
            )}
        </Container>
    );
};
