import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import auth from './auth';
import Login from './Login';
import Callback from './Callback';
import Articles from './Articles';

const App = () => {
  const [user, setUser] = useState(null);

  useEffect(() => {
    auth.getUser().then((user) => {
      setUser(user);
    });
  }, []);

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Articles user={user} />} />
        <Route path="/login" element={<Login />} />
        <Route path="/callback" element={<Callback />} />
      </Routes>
    </Router>
  );
};

export default App;
