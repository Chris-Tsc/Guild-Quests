import { useState } from 'react';
import type { SubmitEventHandler } from 'react';
import { useNavigate } from 'react-router';
import { apiRequest } from '../api/auth';

function CreatePlayerPage() {
    const navigate = useNavigate();

    const [inGameName, setInGameName] = useState('');
    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit: SubmitEventHandler<HTMLFormElement> = async (event) => {
        event.preventDefault();
        setError('');
        setIsSubmitting(true);

        try {
            await apiRequest('/Player/create', {
                method: 'POST',
                body: JSON.stringify({ inGameName }),
            });

            navigate('/menu');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Could not create player.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <main className="page">
            <form className="auth-stack" onSubmit={handleSubmit}>
                <h1>Create Your Adventurer</h1>

                <input
                    type="text"
                    placeholder="Player name"
                    value={inGameName}
                    onChange={(event) => setInGameName(event.target.value)}
                />

                {error && <p className="form-error">{error}</p>}

                <button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? 'Creating...' : 'Begin'}
                </button>
            </form>
        </main>
    );
}

export default CreatePlayerPage;