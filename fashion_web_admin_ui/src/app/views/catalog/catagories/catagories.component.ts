import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  AdminApiCategoryApiClient,
  CategoryDTO,
  CreateCategoryDTO,
  UpdateCategoryDTO
} from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';
import { DataCacheService } from '../../../shared/services/data-cache.service';

interface CategoryViewModel {
  id: string;
  name: string;
  description: string;
}

@Component({
  selector: 'app-catagories',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './catagories.component.html',
  styleUrl: './catagories.component.scss',
})
export class CatagoriesComponent implements OnInit, AfterViewInit {
  categories: CategoryViewModel[] = [];
  private readonly listCacheTtlMs = 3 * 60 * 1000;
  private categoriesLoadRequestId = 0;

  isLoading = false;
  isSaving = false;
  isDeleting = false;

  editingCategoryId: string | null = null;

  readonly categoryForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(1000)]]
  });

  constructor(
    private fb: FormBuilder,
    private categoryApi: AdminApiCategoryApiClient,
    private alertService: AlertService,
    private dataCacheService: DataCacheService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Keep ngOnInit lightweight to avoid expression-changed errors on first check.
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadCategories();
    }, 0);
  }

  get canSubmit(): boolean {
    return this.categoryForm.valid && !this.isSaving;
  }

  get isEditMode(): boolean {
    return this.editingCategoryId !== null;
  }

  loadCategories(forceRefresh = false): void {
    const requestId = ++this.categoriesLoadRequestId;
    this.isLoading = true;
    if (forceRefresh) {
      this.dataCacheService.invalidateByPrefix('catalog:categories');
    }

    this.dataCacheService.getOrSet(
      'catalog:categories',
      () => this.categoryApi.categoriesGET(),
      this.listCacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.categoriesLoadRequestId) {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.categoriesLoadRequestId) {
            return;
          }

          const list = response.data ?? [];
          this.categories = list.map((item, index) => this.toCategoryViewModel(item, index));
          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai danh sach danh muc.');
        }
      });
  }

  startCreate(): void {
    this.editingCategoryId = null;
    this.categoryForm.reset({
      name: '',
      description: ''
    });
  }

  startEdit(category: CategoryViewModel): void {
    this.editingCategoryId = category.id;
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description
    });
  }

  saveCategory(): void {
    if (!this.canSubmit || this.isSaving) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const formValue = this.categoryForm.getRawValue();
    this.isSaving = true;

    if (this.isEditMode && this.editingCategoryId) {
      const dto = new UpdateCategoryDTO({
        name: formValue.name ?? '',
        description: formValue.description ?? ''
      });

      this.categoryApi.categoriesPUT(this.editingCategoryId, dto)
        .pipe(finalize(() => {
          this.isSaving = false;
        }))
        .subscribe({
          next: response => {
            this.alertService.success(response.message ?? 'Cap nhat danh muc thanh cong.');
            this.invalidateCategoryCache();
            this.startCreate();
            this.loadCategories();
          },
          error: () => {
            this.alertService.error('Cap nhat danh muc that bai.');
          }
        });
      return;
    }

    const dto = new CreateCategoryDTO({
      name: formValue.name ?? '',
      description: formValue.description ?? ''
    });

    this.categoryApi.categoriesPOST(dto)
      .pipe(finalize(() => {
        this.isSaving = false;
      }))
      .subscribe({
        next: response => {
          this.alertService.success(response.message ?? 'Them danh muc thanh cong.');
          this.invalidateCategoryCache();
          this.startCreate();
          this.loadCategories();
        },
        error: () => {
          this.alertService.error('Them danh muc that bai.');
        }
      });
  }

  deleteCategory(category: CategoryViewModel): void {
    const confirmed = globalThis.confirm(`Xoa danh muc "${category.name}"?`);
    if (!confirmed) {
      return;
    }

    this.isDeleting = true;
    this.categoryApi.categoriesDELETE(category.id)
      .pipe(finalize(() => {
        this.isDeleting = false;
      }))
      .subscribe({
        next: response => {
          this.alertService.success(response.message ?? 'Xoa danh muc thanh cong.');
          this.invalidateCategoryCache();
          this.loadCategories();
        },
        error: () => {
          this.alertService.error('Xoa danh muc that bai.');
        }
      });
  }

  private toCategoryViewModel(item: CategoryDTO, index: number): CategoryViewModel {
    return {
      id: item.id ?? `category-${index + 1}`,
      name: item.name ?? '',
      description: item.description ?? ''
    };
  }

  private invalidateCategoryCache(): void {
    this.dataCacheService.invalidateByPrefix('catalog:categories');
    this.dataCacheService.invalidateByPrefix('dashboard:categories');
  }

}
