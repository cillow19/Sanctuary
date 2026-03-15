import express from "express";
import path from "path";
import { fileURLToPath } from "url";


const router = express.Router();

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

router.get("/clientmanifest.xml", (req, res) => {
    res.setHeader("Content-Type", "text/xml");
    res.sendFile(path.join(__dirname, "..", "manifests", "clientmanifest.xml"));
});

router.get("/servermanifest.xml", (req, res) => {
    res.setHeader("Content-Type", "text/xml");
    res.sendFile(path.join(__dirname, "..", "manifests", "servermanifest.xml"));
});

export default router;
