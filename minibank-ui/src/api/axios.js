import axios from 'axios';

const api = axios.create({
    baseURL: 'http://minibank.local/api'
});
export default api;