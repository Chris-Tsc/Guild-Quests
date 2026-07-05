import { useState } from 'react';
import type { SubmitEventHandler } from 'react';
import { Link, useNavigate } from 'react-router';
import { apiRequest, getMyPlayer, isApiStatusError, setToken } from '../api/auth';

type AuthResponse = {
    token: string;
};

function RegisterPage() {
    const navigate = useNavigate();

    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit: SubmitEventHandler<HTMLFormElement> = async (event) => {
        event.preventDefault();
        setError('');
        setIsSubmitting(true);

        try {
            const result = await apiRequest<AuthResponse>('/App/register', {
                method: 'POST',
                body: JSON.stringify({ username, password }),
            });

            setToken(result.token);

            try {
                await getMyPlayer();
                navigate('/menu');
            } catch (playerError) {
                if (isApiStatusError(playerError) && playerError.status === 404) {
                    navigate('/create-player');
                    return;
                }

                throw playerError;
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Registration failed.');
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <main className="page">
            <form className="auth-stack" onSubmit={handleSubmit}>
                <h1>Create Account</h1>

                <input
                    type="text"
                    placeholder="Username"
                    value={username}
                    onChange={(event) => setUsername(event.target.value)}
                />

                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                />

                {error && <p className="form-error">{error}</p>}

                <button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? 'Creating...' : 'Register'}
                </button>

                <Link to="/login">Back to login</Link>
            </form>
        </main>
    );
}

export default RegisterPage;