import { Navigate, Route, Routes } from 'react-router';
import './App.css';

import GuildPage from './pages/GuildPage';
import LoginPage from './pages/LoginPage';
import MainMenuPage from './pages/MainMenuPage';
import ProfilePage from './pages/ProfilePage';
import QuestsPage from './pages/QuestsPage';
import RegisterPage from './pages/RegisterPage';
import StatsPage from './pages/StatsPage';
import CreatePlayerPage from './pages/CreatePlayerPage';

function App() {
    return (
        <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/create-player" element={<CreatePlayerPage />} />
            <Route path="/menu" element={<MainMenuPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/stats" element={<StatsPage />} />
            <Route path="/quests" element={<QuestsPage />} />
            <Route path="/guild" element={<GuildPage />} />
        </Routes>
    );
}

export default App;