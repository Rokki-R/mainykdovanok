import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Container, Card, Spinner } from 'react-bootstrap';
import toast from 'react-hot-toast';
import axios from 'axios';

export const UserProfilePage = () => {
    const { userId } = useParams();
    const [image, setImage] = useState(null);
    const [user, setUser] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const userDetailsResponse = await axios.get(`api/user/getUserDetails/${userId}`);
                setUser(userDetailsResponse.data);
                const userProfileImageResponse = await axios.get(`api/user/getUserProfileImage/${userId}`);
                setImage(userProfileImageResponse.data);
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
                <Card className="my-4" style={{ width: '350px' }}>
                    <Card.Body className="text-center">
                        <div>
                            {image ? (
                                <img
                                    src={`data:image/png;base64,${image.user_profile_image}`}
                                    alt="Profile"
                                    className="rounded-circle mb-3"
                                    style={{ width: '200px', height: '200px' }}
                                />
                            ) : (
                                <div className="rounded-circle bg-secondary mb-3" style={{ width: '200px', height: '200px' }} />
                            )}
                        </div>
                        <Card.Title className="mb-4">
                            {user.name} {user.surname}
                        </Card.Title>
                        <Card.Text className="text-center mb-0">
                            <strong>El. paštas:</strong> {user.email}
                        </Card.Text>
                    </Card.Body>
                </Card>
            )}
        </Container>
    );
};
