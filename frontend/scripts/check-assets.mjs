import assert from 'node:assert/strict';
import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { services } from '../src/data/salonData.js';
import { mapServiceFromApi, normalizeImageUrl } from '../src/api/mappers.js';

const root = new URL('../public/', import.meta.url);
const paths = new Set(services.map((service) => service.image));
for (const file of readdirSync(new URL('service-images/', root))) paths.add(`/service-images/${file}`);
paths.add('/logo.jpg');
paths.add('/photo_2026-08-20_00-29-03.jpg');
paths.add('/photo_2026-08-20_00-29-05.jpg');

for (const path of paths) {
  const file = new URL(path.slice(1), root);
  assert.ok(existsSync(file), `Missing public image: ${path}`);
  const bytes = readFileSync(file);
  assert.ok(bytes[0] === 0xff && bytes[1] === 0xd8, `Invalid JPEG header: ${path}`);
}

assert.equal(normalizeImageUrl('public/logo.jpg'), '/logo.jpg');
assert.equal(normalizeImageUrl('/logo.jpg'), '/logo.jpg');
assert.equal(normalizeImageUrl('src/public/service-images/public/example.jpg'), '/service-images/example.jpg');
assert.equal(normalizeImageUrl('C:\\site\\frontend\\public\\service-images\\example.jpg'), '/service-images/example.jpg');
assert.equal(normalizeImageUrl(' https://example.com/image.jpg '), 'https://example.com/image.jpg');
assert.equal(normalizeImageUrl('javascript:alert(1)'), normalizeImageUrl(null));
const mapped = mapServiceFromApi({ serviceId: 42, serviceName: 'Custom', imageUrl: '/custom.jpg' });
assert.equal(mapped.id, '42');
assert.equal(mapped.image, '/custom.jpg');
for (const name of ['README.md', 'DATABASE-ADMIN-GUIDE.md']) {
  const content = readFileSync(new URL(`../../${name}`, import.meta.url));
  assert.ok(content.length > 0 && !content.includes(0), `Empty or corrupted documentation: ${name}`);
}
const backendCatalog = readFileSync(new URL('../../backend/Data/ServiceCatalog.cs', import.meta.url), 'utf8');
const backendEntries = [...backendCatalog.matchAll(/S\(\d+, "([^"]+)", "([^"]+)"[^\n]+, "([^"\n]+\.jpg)"\)/g)];
assert.equal(backendEntries.length, services.length, 'Backend/sample catalog size mismatch');
for (const entry of backendEntries) {
  const service = services.find((item) => item.name === entry[1] && item.category === entry[2]);
  assert.ok(service, `Backend service missing from sample catalog: ${entry[1]}`);
  assert.equal(entry[3], service.image, `Catalog image mismatch: ${entry[1]} (${entry[2]})`);
}
console.log(`PASS: ${services.length} sample services, ${paths.size} JPEG assets, image mapping.`);
