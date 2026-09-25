import { Router, type RequestHandler } from 'express';
import type { AuthService } from '../../../application/auth/services/auth.service.js';
import { userContext } from '../request-context.js';
import { parse, schemas } from '../validation.js';

/** /api/auth */
export function authRoutes(auth: AuthService, requireSession: RequestHandler): Router {
  const router = Router();

  router.post('/register', async (req, res) => {
    res.status(201).json(await auth.register(parse(schemas.register, req.body)));
  });

  router.post('/login', async (req, res) => {
    res.json(await auth.authenticate(parse(schemas.login, req.body)));
  });

  router.get('/me', requireSession, async (req, res) => {
    res.json(await auth.getUser({ context: userContext(req) }));
  });

  return router;
}
