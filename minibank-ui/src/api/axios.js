import axios from 'axios';

const api = axios.create({
    baseURL: 'http://10.81.172.207:5000/api'
});
export default api;