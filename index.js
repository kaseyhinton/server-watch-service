const polka = require("polka");
const cors = require("cors");
var tcpp = require("tcp-ping");

const app = polka();

const ONE_MINUTE = 60 * 1000;
const serverCache = {};

app.use(cors());
app.get("/server/:ip", async (req, res) => {
  const { ip } = req.params;

  if (serverCache?.[ip]) {
    const lastPing = serverCache?.[ip];
    if (new Date() - new Date(lastPing?.date) < ONE_MINUTE) {
      res.end(lastPing?.status ?? "Error");
    }
  }

  tcpp.ping({ address: ip, attempts: 1 }, function (err, data) {
    const status = data?.results?.[0]?.time !== undefined;
    serverCache[ip] = {
      status: status,
      date: new Date(),
    };
    res.end(status ? "Online" : "Offline");
  });
});

const port = process.env["PORT"] || 3000;
app.listen(port, () => {
  console.log(`> Running on localhost:${port}`);
});
