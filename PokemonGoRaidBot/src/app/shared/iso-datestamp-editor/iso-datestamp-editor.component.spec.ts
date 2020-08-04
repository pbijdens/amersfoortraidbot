import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IsoDatestampEditorComponent } from './iso-datestamp-editor.component';

describe('IsoDatestampEditorComponent', () => {
  let component: IsoDatestampEditorComponent;
  let fixture: ComponentFixture<IsoDatestampEditorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IsoDatestampEditorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IsoDatestampEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
