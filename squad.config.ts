import {
  defineSquad,
  defineTeam,
  defineAgent,
} from '@bradygaster/squad-sdk';

/**
 * Squad Configuration — showcase-results
 */
const scribe = defineAgent({
  name: 'scribe',
  role: 'scribe',
  description: 'Scribe',
  status: 'active',
});

export default defineSquad({
  version: '1.0.0',

  team: defineTeam({
    name: 'showcase-results',
    members: ['scribe'],
  }),

  agents: [scribe],
});
